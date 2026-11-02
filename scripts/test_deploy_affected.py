"""Offline regression tests for health-gated, selective Docker deployment and rollback."""
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

OLD = "sha256:" + "a" * 64
NEW = "sha256:" + "b" * 64
CALCULATOR_NEW = "sha256:" + "c" * 64


class FakeDocker:
    def __init__(self, previous=None, *, failure=None):
        self.images = dict(previous if previous is not None else {
            "goldex-app": OLD, "fani": OLD, "karat": OLD,
        })
        self.health = {name: "healthy" for name in self.images}
        self.calls = []
        self.overrides = []
        self.failure = failure
        self.fail_done = False
        self.stopped = []
        self.nginx_reloads = 0

    def new_id(self, name):
        return CALCULATOR_NEW if name == "karat" else NEW

    def __call__(self, args, **kwargs):
        args = list(args)
        self.calls.append(args)
        if args[:2] == ["docker", "compose"]:
            if args[-3:] == ["config", "--format", "json"]:
                return subprocess.CompletedProcess(args, 0, json.dumps(CONFIG))
            if args[-2:] == ["config", "--quiet"]:
                if self.failure == "config" and not self.fail_done:
                    self.fail_done = True
                    raise subprocess.CalledProcessError(1, args)
                return subprocess.CompletedProcess(args, 0)
            if "ps" in args:
                name = args[-1]
                return subprocess.CompletedProcess(args, 0, name + "-id" if name in self.images else "")
            if "up" in args:
                overlay_path = Path(args[args.index("-f", args.index("-f") + 1) + 1])
                overlay = json.loads(overlay_path.read_text(encoding="utf-8"))
                self.overrides.append(overlay)
                for name, service in overlay["services"].items():
                    image = service["image"]
                    self.images[name] = image if image.startswith("sha256:") else self.new_id(name)
                    self.health[name] = "healthy"
                    if self.failure == "not-ready" and image.startswith("reg.") and not self.fail_done:
                        self.health[name] = "unhealthy"
                if self.failure == "up" and not self.fail_done:
                    self.fail_done = True
                    raise subprocess.CalledProcessError(1, args)
                return subprocess.CompletedProcess(args, 0)
            if "stop" in args:
                self.stopped.append(args[-1])
                self.images.pop(args[-1], None)
                return subprocess.CompletedProcess(args, 0)
            raise AssertionError("Unexpected Compose command " + repr(args))
        if args[:3] == ["docker", "image", "inspect"]:
            image = args[3]
            if args[-1] == "{{.Id}}":
                if image.startswith("sha256:"):
                    ident = image
                else:
                    ident = CALCULATOR_NEW if "karat" in image else NEW
                return subprocess.CompletedProcess(args, 0, ident)
            if "ir.goldex.health.ready" in args[-1]:
                return subprocess.CompletedProcess(args, 0, "true" if image != OLD else "<no value>")
        if args[:3] == ["docker", "container", "inspect"]:
            name = args[3].replace("-id", "")
            if name not in self.images:
                raise subprocess.CalledProcessError(1, args)
            value = args[-1]
            if value == "{{.Image}}":
                return subprocess.CompletedProcess(args, 0, self.images[name])
            if value == "{{json .State}}":
                return subprocess.CompletedProcess(
                    args, 0, json.dumps({"Running": True, "Health": {"Status": self.health[name]}})
                )
        if args[:2] == ["docker", "pull"]:
            if self.failure == "pull" and not self.fail_done:
                self.fail_done = True
                raise subprocess.CalledProcessError(1, args)
        if args[:3] == ["docker", "exec", "nginx"]:
            if args[-2:] == ["nginx", "-s"] or args[-1] == "reload":
                self.nginx_reloads += 1
            if self.failure == "nginx" and not self.fail_done:
                self.fail_done = True
                raise subprocess.CalledProcessError(1, args)
        return subprocess.CompletedProcess(args, 0, "")


def run_deploy(apps, fake, *, expect_failure=False):
    with tempfile.TemporaryDirectory() as root:
        root = Path(root)
        compose = root / "compose.yaml"
        compose.write_text("services: {}", encoding="utf-8")
        with patch.object(deployment.subprocess, "run", side_effect=fake):
            if expect_failure:
                with unittest.TestCase().assertRaises((RuntimeError, subprocess.CalledProcessError)):
                    deployment.deploy(compose, "1.2.155.12345", apps, root / "apps")
            else:
                deployment.deploy(compose, "1.2.155.12345", apps, root / "apps")
    return fake


class DeploymentTests(unittest.TestCase):
    def test_all_tenants_selected_without_touching_database(self):
        self.assertEqual(
            {"goldex-app", "fani"},
            set(deployment.selected_services(CONFIG, ["goldex"]))
        )
        self.assertEqual({"karat"}, set(deployment.selected_services(CONFIG, ["calculator"])))
        self.assertEqual(
            {"goldex-app", "fani", "karat"},
            set(deployment.selected_services(CONFIG, ["goldex", "calculator"]))
        )
        with self.assertRaises(ValueError):
            deployment.selected_services({"services": {}}, ["goldex"])

    def test_success_requires_explicit_health_and_database_probe_for_every_service(self):
        fake = run_deploy(["goldex", "calculator"], FakeDocker())
        self.assertEqual(
            {"goldex-app": NEW, "fani": NEW, "karat": CALCULATOR_NEW}, fake.images
        )
        self.assertEqual(1, fake.nginx_reloads)
        self.assertEqual({"goldex-app", "fani", "karat"}, set(fake.overrides[0]["services"]))
        for service in fake.overrides[0]["services"].values():
            self.assertIn("/health/ready", service["healthcheck"]["test"][1])
            self.assertIn("200", service["healthcheck"]["test"][1])
        calls = fake.calls
        self.assertEqual(2, len([a for a in calls if a[:2] == ["docker", "pull"]]))
        self.assertLess(
            max(i for i, a in enumerate(calls) if a[:2] == ["docker", "pull"]),
            min(i for i, a in enumerate(calls) if "up" in a),
        )
        self.assertFalse(any("sqldb" in a for a in calls))
        self.assertFalse(any("--remove-orphans" in a for a in calls))
        self.assertTrue(all("--no-deps" in a for a in calls if "up" in a))

    def test_calculator_only_does_not_touch_enterprise(self):
        fake = run_deploy(["calculator"], FakeDocker())
        self.assertEqual(OLD, fake.images["goldex-app"])
        self.assertEqual(OLD, fake.images["fani"])
        self.assertEqual(CALCULATOR_NEW, fake.images["karat"])
        self.assertFalse(any(a[0] == "sudo" for a in fake.calls))

    def test_unhealthy_status_rolls_back_before_reporting_success(self):
        fake = run_deploy(["goldex"], FakeDocker(failure="not-ready"), expect_failure=True)
        self.assertEqual(OLD, fake.images["goldex-app"])
        self.assertEqual(OLD, fake.images["fani"])
        self.assertGreaterEqual(fake.nginx_reloads, 1)
        self.assertEqual(2, len(fake.overrides))
        self.assertEqual(
            {"disable": True}, fake.overrides[1]["services"]["fani"]["healthcheck"]
        )

    def test_compose_wait_failure_restores_every_selected_service(self):
        fake = run_deploy(["goldex", "calculator"], FakeDocker(failure="up"), expect_failure=True)
        self.assertEqual({"goldex-app": OLD, "fani": OLD, "karat": OLD}, fake.images)
        self.assertEqual(2, len(fake.overrides))

    def test_nginx_failure_rolls_back_containers_and_reloads_proxy(self):
        fake = run_deploy(["calculator"], FakeDocker(failure="nginx"), expect_failure=True)
        self.assertEqual(OLD, fake.images["karat"])
        self.assertGreaterEqual(fake.nginx_reloads, 1)

    def test_bad_pull_or_config_never_updates_live_services(self):
        for failure in ("config", "pull"):
            with self.subTest(failure=failure):
                fake = run_deploy(["calculator"], FakeDocker(failure=failure), expect_failure=True)
                self.assertEqual(OLD, fake.images["karat"])
                self.assertFalse(fake.overrides)

    def test_first_deployment_failure_stops_failed_container(self):
        fake = run_deploy(
            ["calculator"], FakeDocker(previous={}, failure="up"), expect_failure=True
        )
        self.assertIn("karat", fake.stopped)
        self.assertNotIn("karat", fake.images)

    def test_version_and_nginx_container_are_validated_first(self):
        with patch.object(deployment.subprocess, "run") as mocked:
            for version in ("latest", "1.0.1.0", "1.2.01.1", "1.2.1.1;foo"):
                with self.subTest(version=version), self.assertRaises(ValueError):
                    deployment.deploy(Path("not-present"), version, ["goldex"], Path("apps"))
            with self.assertRaises(ValueError):
                deployment.deploy(Path("not-present"), "1.2.1.1", ["goldex"], Path("apps"), "nginx;sudo")
            mocked.assert_not_called()

    @unittest.skipUnless(shutil.which("docker"), "Docker CLI not installed")
    def test_real_compose_health_override_preserves_existing_service_environment(self):
        with tempfile.TemporaryDirectory() as temp:
            root = Path(temp)
            compose_file = root / "compose.json"
            compose_file.write_text(json.dumps({
                "name": "goldex-health-gate-test",
                "services": {
                    "karat": {
                        "image": "reg.goldexsoft.ir/goldex-karat:latest",
                        "environment": {"SHOP": "${GOLDEX_TEST_SHOP}"},
                        "volumes": ["shop-data:/data"],
                    }
                },
                "volumes": {"shop-data": {}},
            }))
            (root / ".env").write_text("GOLDEX_TEST_SHOP=from-server-env\n")
            override = root / "health.json"
            deployment.write_override(
                override, {"karat": "reg.goldexsoft.ir/goldex-karat:1.2.155.12345"},
                {"karat": True},
            )
            result = subprocess.run([
                "docker", "compose", "--project-directory", str(root),
                "-f", str(compose_file), "-f", str(override),
                "config", "--format", "json",
            ], capture_output=True, text=True)
            if result.returncode != 0:
                self.fail("Compose rejected readiness override: " + result.stderr)
            svc = json.loads(result.stdout)["services"]["karat"]
            self.assertEqual("from-server-env", svc["environment"]["SHOP"])
            self.assertEqual("/data", svc["volumes"][0]["target"])
            self.assertIn("/health/ready", svc["healthcheck"]["test"][1])
            self.assertIn("| grep -qx 200", svc["healthcheck"]["test"][1])
            self.assertNotIn("$", svc["healthcheck"]["test"][1])


if __name__ == "__main__":
    unittest.main()