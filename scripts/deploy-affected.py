"""Deploy selected applications using the existing production Compose configuration."""

import argparse
import json
import os
from pathlib import Path
import re
import subprocess
import tempfile


IMAGES = {
    "goldex": "reg.goldexsoft.ir/goldex",
    "calculator": "reg.goldexsoft.ir/goldex-karat",
}


def repository(image):
    image = image.split("@", 1)[0]
    prefix, separator, name = image.rpartition("/")
    return (prefix + separator if separator else "") + name.split(":", 1)[0]


def selected_services(config, applications):
    selected = {}
    for app in applications:
        matches = [
            name for name, service in config["services"].items()
            if repository(service.get("image", "")) == IMAGES[app]
        ]
        if not matches:
            raise ValueError(f"No Compose service uses {IMAGES[app]}; deployment stopped.")
        for name in matches:
            selected[name] = IMAGES[app]
    return selected


def deploy(compose_file, version, applications, app_base):
    if not re.fullmatch(r"1\.2\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)", version):
        raise ValueError("Deployment requires an immutable 1.2 Docker build version.")
    compose_file = compose_file.resolve(strict=True)
    environment = dict(os.environ, VERSION=version)
    compose = ["docker", "compose", "--project-directory", str(compose_file.parent), "-f", str(compose_file)]
    result = subprocess.run(
        [*compose, "config", "--format", "json"], env=environment,
        cwd=compose_file.parent, check=True, capture_output=True, text=True,
    )
    services = selected_services(json.loads(result.stdout), applications)
    # Pin selected images even if the server file uses :latest or another VERSION default.
    override = {"services": {name: {"image": f"{image}:{version}"} for name, image in services.items()}}
    with tempfile.TemporaryDirectory(prefix="goldex-deploy-") as directory:
        path = Path(directory) / "images.json"
        path.write_text(json.dumps(override), encoding="utf-8")
        command = [*compose, "-f", str(path)]
        # Validate all selected services before pulling or recreating anything.
        subprocess.run([*command, "config", "--quiet"], cwd=compose_file.parent, env=environment, check=True)
        if "goldex" in applications:
            directories = [app_base / item for item in (
                "shared/content", "shared/posts", "shared/categories", "fani/uploads", "fani/Reports",
            )]
            subprocess.run(["sudo", "mkdir", "-p", *map(str, directories)], check=True)
            # Preserve the permissions policy of the existing server refresh script.
            subprocess.run(["sudo", "chmod", "-R", "777", str(app_base)], check=True)
        for image in dict.fromkeys(services.values()):
            subprocess.run(["docker", "pull", f"{image}:{version}"], check=True)
        # No orphan removal or dependency recreation: unrelated applications keep running.
        subprocess.run([
            *command, "up", "-d", "--no-deps", "--no-build", "--pull", "never",
            "--wait", "--wait-timeout", "180", *services,
        ], cwd=compose_file.parent, env=environment, check=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compose-file", type=Path, default=Path("/home/user/docker/goldex/docker-compose.yaml"))
    parser.add_argument("--app-base", type=Path, default=Path("/var/volumes/apps/goldex"))
    parser.add_argument("--version", required=True)
    parser.add_argument("--app", choices=list(IMAGES), action="append", required=True)
    args = parser.parse_args()
    deploy(args.compose_file, args.version, args.app, args.app_base)


if __name__ == "__main__":
    main()
