import importlib.util
import json
from pathlib import Path
import shutil
import subprocess
import tempfile
import unittest
from unittest.mock import patch


spec = importlib.util.spec_from_file_location("deploy_affected", Path(__file__).with_name("deploy-affected.py"))
deployment = importlib.util.module_from_spec(spec)
spec.loader.exec_module(deployment)


CONFIG = {"services": {
    "goldex-app": {"image": "reg.goldexsoft.ir/goldex:latest"},
    "fani": {"image": "reg.goldexsoft.ir/goldex:1.2.1.1"},
    "karat": {"image": "reg.goldexsoft.ir/goldex-karat:latest"},
    "sqldb": {"image": "mcr.microsoft.com/mssql/server:2022-latest"},
}}


class SelectiveDeploymentTests(unittest.TestCase):
    @unittest.skipUnless(shutil.which("docker"), "Docker CLI is not installed")
    def test_real_compose_merges_override_preserves_mounts_and_uses_server_env(self):
        real_run = subprocess.run
        with tempfile.TemporaryDirectory() as directory:
            root = Path(directory)
            compose_file = root / "compose.json"
            compose_file.write_text(json.dumps({
                "name": "goldex-deployment-test",
                "services": {
                    "goldex-app": {"image": "reg.goldexsoft.ir/goldex:latest"},
                    "karat": {
                        "image": "reg.goldexsoft.ir/goldex-karat:${VERSION:-latest}",
                        "environment": {"SHOP": "${GOLDEX_TEST_SHOP}"},
                        "volumes": ["shop-data:/data"],
                    },
                },
                "volumes": {"shop-data": {}},
            }), encoding="utf-8")
            (root / ".env").write_text("GOLDEX_TEST_SHOP=compose-test\n", encoding="utf-8")
            updates = []

            def command(args, **kwargs):
                if args[:2] == ["docker", "compose"]:
                    if "up" in args:
                        updates.append(args)
                        # Inspect the actual merged Compose model; never start a container.
                        merged = real_run(
                            [*args[:args.index("up")], "config", "--format", "json"],
                            **kwargs, capture_output=True, text=True,
                        )
                        services = json.loads(merged.stdout)["services"]
                        self.assertEqual("reg.goldexsoft.ir/goldex-karat:1.2.149.10", services["karat"]["image"])
                        self.assertEqual("reg.goldexsoft.ir/goldex:latest", services["goldex-app"]["image"])
                        self.assertEqual("compose-test", services["karat"]["environment"]["SHOP"])
                        self.assertEqual("/data", services["karat"]["volumes"][0]["target"])
                    else:
                        return real_run(args, **kwargs)
                return subprocess.CompletedProcess(args, 0)

            with patch.object(deployment.subprocess, "run", side_effect=command):
                deployment.deploy(compose_file, "1.2.149.10", ["calculator"], root / "apps")
            self.assertEqual(1, len(updates))
            self.assertEqual("karat", updates[0][-1])

    def test_service_mapping_includes_all_tenants_only_of_selected_app(self):
        self.assertEqual({"goldex-app", "fani"}, set(deployment.selected_services(CONFIG, ["goldex"])))
        self.assertEqual({"karat"}, set(deployment.selected_services(CONFIG, ["calculator"])))
        self.assertEqual({"goldex-app", "fani", "karat"}, set(deployment.selected_services(CONFIG, list(deployment.IMAGES))))

    def test_unknown_image_stops_deployment(self):
        with self.assertRaises(ValueError):
            deployment.selected_services({"services": {}}, ["calculator"])

    def test_image_repository_supports_ports_and_digests(self):
        self.assertEqual("localhost:5000/goldex", deployment.repository("localhost:5000/goldex:latest"))
        self.assertEqual("reg.goldexsoft.ir/goldex", deployment.repository("reg.goldexsoft.ir/goldex@sha256:abcd"))

    def run_deployment(self, apps, failure=None):
        calls, overlays = [], []

        def command(args, **kwargs):
            calls.append((args, kwargs))
            if failure and failure(args):
                raise subprocess.CalledProcessError(1, args)
            if args[-3:] == ["config", "--format", "json"]:
                return subprocess.CompletedProcess(args, 0, json.dumps(CONFIG))
            if args[-2:] == ["config", "--quiet"]:
                overlays.append(json.loads(Path(args[-3]).read_text(encoding="utf-8")))
            return subprocess.CompletedProcess(args, 0)

        with tempfile.TemporaryDirectory() as directory:
            compose = Path(directory) / "docker-compose.yaml"
            compose.write_text("services: {}", encoding="utf-8")
            with patch.object(deployment.subprocess, "run", side_effect=command):
                if failure:
                    with self.assertRaises(subprocess.CalledProcessError):
                        deployment.deploy(compose, "1.2.149.10", apps, Path(directory) / "apps")
                else:
                    deployment.deploy(compose, "1.2.149.10", apps, Path(directory) / "apps")
        return calls, overlays

    def test_calculator_only_never_pulls_or_updates_goldex_or_database(self):
        calls, overlays = self.run_deployment(["calculator"])
        self.assertEqual({"services": {"karat": {"image": "reg.goldexsoft.ir/goldex-karat:1.2.149.10"}}}, overlays[0])
        pulls = [args for args, _ in calls if args[:2] == ["docker", "pull"]]
        self.assertEqual([["docker", "pull", "reg.goldexsoft.ir/goldex-karat:1.2.149.10"]], pulls)
        update, options = calls[-1]
        self.assertEqual("karat", update[-1])
        self.assertIn("--no-deps", update)
        self.assertIn("--no-build", update)
        self.assertIn("--wait", update)
        self.assertNotIn("--remove-orphans", update)
        self.assertNotIn("goldex-app", update)
        self.assertNotIn("sqldb", update)
        self.assertFalse(any(args[0] == "sudo" for args, _ in calls))
        self.assertEqual("1.2.149.10", options["env"]["VERSION"])

    def test_enterprise_only_preserves_directory_preparation_and_pins_all_instances(self):
        calls, overlays = self.run_deployment(["goldex"])
        self.assertEqual({"goldex-app", "fani"}, set(overlays[0]["services"]))
        self.assertTrue(any(args[:2] == ["sudo", "mkdir"] for args, _ in calls))
        self.assertEqual(["goldex-app", "fani"], calls[-1][0][-2:])
        self.assertFalse(any("karat" in argument for args, _ in calls for argument in args))

    def test_both_images_are_pulled_before_service_recreation(self):
        calls, _ = self.run_deployment(["goldex", "calculator"])
        self.assertEqual(2, sum(args[:2] == ["docker", "pull"] for args, _ in calls))
        self.assertEqual(["goldex-app", "fani", "karat"], calls[-1][0][-3:])

    def test_config_or_pull_failure_prevents_container_recreation(self):
        for failure in (
            lambda args: args[-2:] == ["config", "--quiet"],
            lambda args: args[:2] == ["docker", "pull"],
        ):
            with self.subTest(failure=failure):
                calls, _ = self.run_deployment(["calculator"], failure)
                self.assertFalse(any("up" in args for args, _ in calls))

    def test_up_failure_propagates(self):
        self.run_deployment(["calculator"], lambda args: "up" in args)

    def test_mutable_or_invalid_version_is_rejected_before_docker(self):
        with patch.object(deployment.subprocess, "run") as run:
            for version in ("latest", "1.0.1.0", "1.2.01.1", "1.2.1.1;echo bad"):
                with self.subTest(version=version), self.assertRaises(ValueError):
                    deployment.deploy(Path("not-present"), version, ["goldex"], Path("apps"))
            run.assert_not_called()


if __name__ == "__main__":
    unittest.main()
