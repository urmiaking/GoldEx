import importlib.util
from datetime import datetime, timedelta, timezone
from pathlib import Path
import unittest
from unittest.mock import patch
import subprocess
import sys
import tempfile


spec = importlib.util.spec_from_file_location("docker_version", Path(__file__).with_name("docker-version.py"))
versions = importlib.util.module_from_spec(spec)
spec.loader.exec_module(versions)


class DockerVersionTests(unittest.TestCase):
    def test_utc_day_and_millisecond_build_number(self):
        now = datetime(2026, 10, 4, 12, 34, 56, 789000, tzinfo=timezone.utc)
        self.assertEqual("1.2.125.45296789", versions.next_version(now))
        self.assertEqual(versions.next_version(now), versions.next_version(now.astimezone(timezone(timedelta(hours=3, minutes=30)))))

    def test_new_family_is_newer_than_both_legacy_counters(self):
        generated = versions.next_version(versions.EPOCH, ["1.0.99999", "1.1.99999"])
        self.assertEqual("1.2.0.0", generated)

    def test_repeated_build_and_clock_rollback_advance_past_highest_version(self):
        self.assertEqual("1.2.125.5", versions.next_version(versions.EPOCH, ["1.2.125.4", "1.2.125.3"]))

    def test_midnight_rollover(self):
        self.assertEqual("1.2.126.0", versions.next_version(versions.EPOCH, ["1.2.125.86399999"]))

    def test_publishing_older_or_equal_version_is_rejected(self):
        for candidate in ["1.2.125.2", "1.2.125.3"]:
            with self.subTest(candidate=candidate), self.assertRaises(ValueError):
                versions.ensure_newer(candidate, ["1.2.125.3"])

    def test_unsupported_family_and_invalid_override_are_rejected(self):
        for value in ["latest", "1.0.400", "1.2.01.4", "1.2.1;echo bad"]:
            with self.subTest(value=value), self.assertRaises(ValueError):
                versions.ensure_newer(value, [])
        with self.assertRaises(ValueError):
            versions.next_version(versions.EPOCH, ["2.0.0"])

    def test_registry_config_supports_single_and_multi_platform_images(self):
        config = '{"config":{"Labels":{"ir.goldex.build.version":"1.2.125.4"}}}'
        for output in [config, '{"linux/amd64":' + config + ',"linux/arm64":' + config + '}']:
            with self.subTest(output=output), patch.object(versions.subprocess, "run") as run:
                run.return_value.stdout = output
                self.assertTrue(all(value == "1.2.125.4" for value in versions.image_versions("registry/goldex")))
        with patch.object(versions.subprocess, "run", side_effect=subprocess.CalledProcessError(1, "docker")):
            with self.assertRaises(subprocess.CalledProcessError):
                versions.image_versions("registry/goldex")

    def test_inherited_base_image_version_is_not_a_goldex_build(self):
        with patch.object(versions.subprocess, "run") as run:
            run.return_value.stdout = '{"config":{"Labels":{"org.opencontainers.image.version":"24.04"}}}'
            self.assertEqual([], versions.image_versions("registry/goldex"))

    def test_parallel_cli_allocations_share_locked_state_without_touching_version_file(self):
        with tempfile.TemporaryDirectory() as directory:
            state = Path(directory) / "state"
            # A future local allocation also exercises rollback handling through the CLI.
            state.write_text("1.2.99999.1", encoding="ascii")
            command = [sys.executable, str(Path(__file__).with_name("docker-version.py")), "--state-file", str(state)]
            processes = [subprocess.Popen(command, stdout=subprocess.PIPE, stderr=subprocess.PIPE, text=True) for _ in range(3)]
            allocated = []
            for process in processes:
                output, errors = process.communicate(timeout=20)
                self.assertEqual(0, process.returncode, errors)
                allocated.append(output.strip())
            self.assertEqual(["1.2.99999.2", "1.2.99999.3", "1.2.99999.4"], sorted(allocated))
            self.assertEqual("1.2.99999.4", state.read_text(encoding="ascii"))

    def test_cli_check_does_not_consume_a_build_number(self):
        with tempfile.TemporaryDirectory() as directory:
            state = Path(directory) / "state"
            result = subprocess.run([sys.executable, str(Path(__file__).with_name("docker-version.py")), "--check", "1.2.125.1", "--state-file", str(state)], capture_output=True)
            self.assertEqual(0, result.returncode, result.stderr)
            self.assertFalse(state.exists())


if __name__ == "__main__":
    unittest.main()
