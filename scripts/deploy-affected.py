"""Deploy selected GoldEx applications, verify SQL readiness, and roll back on failure.

The server's existing Compose configuration, environment, networks and volumes remain intact.
No persistent deployment state is committed by this helper; the workflow records success
only after this transaction and its Nginx reload have passed.
"""

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
WAIT_TIMEOUT_SECONDS = "240"
HEALTHCHECK = {
    "test": [
        "CMD-SHELL",
        # Exact HTTP 200; no shell variables so Compose cannot interpolate the probe.
        "curl --silent --show-error --output /dev/null --write-out '%{http_code}' "
        "--max-time 5 http://127.0.0.1:8080/health/ready | grep -qx 200",
    ],
    "interval": "15s",
    "timeout": "6s",
    "retries": 8,
    "start_period": "60s",
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


def docker_output(args, *, cwd=None, env=None):
    return subprocess.run(
        args, cwd=cwd, env=env, check=True, capture_output=True, text=True
    ).stdout.strip()


def docker_run(args, *, cwd=None, env=None):
    subprocess.run(args, cwd=cwd, env=env, check=True)


def inspect_image_id(image):
    result = docker_output(["docker", "image", "inspect", image, "--format", "{{.Id}}"])
    if not re.fullmatch(r"sha256:[a-f0-9]{64}", result):
        raise RuntimeError("Docker returned an invalid image ID; refusing an unsafe deployment.")
    return result


def has_readiness_probe(image):
    value = docker_output([
        "docker", "image", "inspect", image, "--format",
        '{{index .Config.Labels "ir.goldex.health.ready"}}',
    ])
    return value == "true"


def container_for_service(compose, name, cwd, env):
    output = docker_output([*compose, "ps", "--all", "--quiet", name], cwd=cwd, env=env)
    ids = output.splitlines()
    if len(ids) > 1:
        raise RuntimeError(f"Multiple containers for {name}; refusing ambiguous rollback.")
    return ids[0] if ids else None


def running_image(compose, name, cwd, env):
    container = container_for_service(compose, name, cwd, env)
    if not container:
        return None
    image_id = docker_output(
        ["docker", "container", "inspect", container, "--format", "{{.Image}}"]
    )
    # Keep the exact local image ID, not a mutable tag such as :latest.
    inspect_image_id(image_id)
    return image_id


def verify_services(compose, expected, cwd, env, require_healthy):
    for service, image_id in expected.items():
        container = container_for_service(compose, service, cwd, env)
        if not container:
            raise RuntimeError(f"{service} did not create a container.")
        state = json.loads(docker_output([
            "docker", "container", "inspect", container, "--format", "{{json .State}}"
        ]))
        current_image = docker_output([
            "docker", "container", "inspect", container, "--format", "{{.Image}}"
        ])
        if not state.get("Running") or current_image != image_id:
            raise RuntimeError(f"{service} is stopped or running an unexpected image.")
        if require_healthy.get(service) and state.get("Health", {}).get("Status") != "healthy":
            raise RuntimeError(f"{service} did not pass its database readiness check.")


def write_override(path, images, readiness):
    path.write_text(json.dumps({
        "services": {
            name: {
                "image": image,
                "healthcheck": HEALTHCHECK if readiness[name] else {"disable": True},
            }
            for name, image in images.items()
        }
    }), encoding="utf-8")


def compose_up(compose, services, cwd, env):
    if not services:
        return
    docker_run([
        *compose, "up", "-d", "--no-deps", "--no-build", "--pull", "never",
        "--force-recreate", "--wait", "--wait-timeout", WAIT_TIMEOUT_SECONDS,
        *services,
    ], cwd=cwd, env=env)


def reload_nginx(container):
    # Test before a graceful reload; the reverse proxy must resolve the new container IP.
    docker_run(["docker", "exec", container, "nginx", "-t"])
    docker_run(["docker", "exec", container, "nginx", "-s", "reload"])


def rollback(compose, override_file, previous, cwd, env, nginx):
    """Restore ALL selected services, including those updated before a partial failure."""
    print("New deployment failed. Restoring previous immutable Docker image IDs.", flush=True)
    retained = {name: image for name, image in previous.items() if image}
    missing = [name for name, image in previous.items() if not image]
    errors = []
    if retained:
        try:
            write_override(
                override_file, retained,
                {name: has_readiness_probe(image) for name, image in retained.items()},
            )
            docker_run([*compose, "config", "--quiet"], cwd=cwd, env=env)
            compose_up(compose, list(retained), cwd, env)
            verify_services(
                compose, retained, cwd, env,
                {name: has_readiness_probe(image) for name, image in retained.items()},
            )
        except (subprocess.CalledProcessError, RuntimeError, ValueError) as exc:
            errors.append(f"Restoration of prior images failed: {exc}")
    for name in missing:
        try:
            # A first installation has no rollback image. Never leave its bad release serving.
            docker_run([*compose, "stop", "--timeout", "30", name], cwd=cwd, env=env)
        except subprocess.CalledProcessError as exc:
            errors.append(f"Stopping first-time service {name} failed: {exc}")
    try:
        reload_nginx(nginx)
    except subprocess.CalledProcessError as exc:
        errors.append(f"Nginx reload after rollback failed: {exc}")
    if errors:
        raise RuntimeError("CRITICAL: rollback incomplete; " + "; ".join(errors))
    print("Previous applications restored; no success state recorded.", flush=True)


def deploy(compose_file, version, applications, app_base, nginx="nginx"):
    if not re.fullmatch(r"1\.2\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)", version):
        raise ValueError("Deployment requires an immutable 1.2 Docker build version.")
    if not nginx or not re.fullmatch(r"[a-zA-Z0-9][a-zA-Z0-9_.-]*", nginx):
        raise ValueError("Invalid Nginx container name.")
    compose_file = compose_file.resolve(strict=True)
    env = dict(os.environ, VERSION=version)
    cwd = compose_file.parent
    base = ["docker", "compose", "--project-directory", str(cwd), "-f", str(compose_file)]
    original = json.loads(docker_output([*base, "config", "--format", "json"], cwd=cwd, env=env))
    services = selected_services(original, applications)

    with tempfile.TemporaryDirectory(prefix="goldex-deploy-") as directory:
        path = Path(directory) / "images.json"
        compose = [*base, "-f", str(path)]

        new_images = {name: f"{repo}:{version}" for name, repo in services.items()}
        write_override(path, new_images, {name: True for name in services})
        docker_run([*compose, "config", "--quiet"], cwd=cwd, env=env)

        # Snapshot every container before pulling or recreating anything.
        previous = {name: running_image(compose, name, cwd, env) for name in services}
        for image in set(filter(None, previous.values())):
            inspect_image_id(image)  # refuse rollout if an old image cannot be restored

        if "goldex" in applications:
            folders = [
                app_base / item for item in
                ("shared/content", "shared/posts", "shared/categories", "fani/uploads", "fani/Reports")
            ]
            docker_run(["sudo", "mkdir", "-p", *map(str, folders)])
            docker_run(["sudo", "chmod", "-R", "777", str(app_base)])

        for repo in dict.fromkeys(services.values()):
            docker_run(["docker", "pull", f"{repo}:{version}"])
        expected_ids = {name: inspect_image_id(image) for name, image in new_images.items()}

        try:
            compose_up(compose, list(services), cwd, env)
            verify_services(
                compose, expected_ids, cwd, env,
                {name: True for name in services},
            )
            reload_nginx(nginx)
        except (subprocess.CalledProcessError, RuntimeError, ValueError) as deployment_error:
            try:
                rollback(base + ["-f", str(path)], path, previous, cwd, env, nginx)
            except (subprocess.CalledProcessError, RuntimeError, ValueError) as rollback_error:
                raise RuntimeError(
                    f"Deployment failed ({deployment_error}); {rollback_error}"
                ) from deployment_error
            raise RuntimeError(
                f"Deployment failed; previous images restored: {deployment_error}"
            ) from deployment_error

        print(f"Readiness verified for {', '.join(services)}; Nginx reloaded.", flush=True)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--compose-file", type=Path, default=Path("/home/user/docker/goldex/docker-compose.yaml"))
    parser.add_argument("--app-base", type=Path, default=Path("/var/volumes/apps/goldex"))
    parser.add_argument("--version", required=True)
    parser.add_argument("--app", choices=list(IMAGES), action="append", required=True)
    parser.add_argument("--nginx-container", default="nginx")
    args = parser.parse_args()
    deploy(args.compose_file, args.version, args.app, args.app_base, args.nginx_container)


if __name__ == "__main__":
    main()