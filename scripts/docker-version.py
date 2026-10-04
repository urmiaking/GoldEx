"""UTC build versions shared by local publishing and CI (Python standard library only)."""

import argparse
from contextlib import contextmanager
from datetime import datetime, timezone
import json
from pathlib import Path
import re
import subprocess
import sys


EPOCH = datetime(2026, 6, 1, tzinfo=timezone.utc)
PREFIX = (1, 2)  # New family sorts above both legacy local/GitHub 1.0 and GitLab 1.1.
MILLISECONDS_PER_DAY = 86_400_000
# Base images can carry an inherited OCI version (e.g. Ubuntu's 24.04).
# Only our dedicated marker is safe as the published GoldEx build floor.
VERSION_LABEL = "ir.goldex.build.version"


def parse_version(value):
    if not re.fullmatch(r"(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)\.(?:0|[1-9]\d*)(?:\.(?:0|[1-9]\d*))?", value):
        raise ValueError(f"Invalid numeric Docker version: {value!r}")
    parts = tuple(map(int, value.split(".")))
    return parts if len(parts) == 4 else (*parts, 0)


def next_version(now, previous=()):
    delta = now.astimezone(timezone.utc) - EPOCH
    if delta.days < 0:
        raise ValueError("Build clock is earlier than the version epoch (2026-06-01 UTC).")
    millis = delta.seconds * 1000 + delta.microseconds // 1000
    candidate = (*PREFIX, delta.days, millis)
    for value in previous:
        floor = parse_version(value)
        if floor[:2] > PREFIX:
            raise ValueError(f"Published version {value} uses a newer version family; update PREFIX first.")
        if floor >= candidate:
            day, tick = divmod(floor[3] + 1, MILLISECONDS_PER_DAY)
            candidate = (*PREFIX, floor[2] + day, tick)
    return ".".join(map(str, candidate))


def image_versions(image):
    # Buildx reads only the remote config, using Docker's existing registry credentials.
    result = subprocess.run(
        ["docker", "buildx", "imagetools", "inspect", f"{image}:latest", "--format", "{{json .Image}}"],
        capture_output=True, text=True, check=True,
    )
    config = json.loads(result.stdout)
    configs = [config] if "config" in config else list(config.values())
    versions = []
    for item in configs:
        labels = item.get("config", {}).get("Labels", {}) or {}
        version = labels.get(VERSION_LABEL)
        if version:
            parse_version(version)
            versions.append(version)
    return versions


def ensure_newer(version, previous):
    candidate = parse_version(version)
    if candidate[:2] != PREFIX:
        family = ".".join(map(str, PREFIX))
        raise ValueError(f"Docker publish versions must use the {family} build family.")
    for value in previous:
        if candidate <= parse_version(value):
            raise ValueError(f"Refusing to publish {version}: {value} is already published.")


@contextmanager
def locked_state(path):
    # Separate invocations in one checkout cannot allocate the same millisecond.
    path.parent.mkdir(parents=True, exist_ok=True)
    with path.open("a+b") as state:
        if sys.platform == "win32":
            import msvcrt
            state.seek(0, 2)
            if state.tell() == 0:
                state.write(b"\n")
                state.flush()
            state.seek(0)
            msvcrt.locking(state.fileno(), msvcrt.LK_LOCK, 1)
        else:
            import fcntl
            fcntl.flock(state.fileno(), fcntl.LOCK_EX)
        try:
            yield state
        finally:
            if sys.platform == "win32":
                state.seek(0)
                msvcrt.locking(state.fileno(), msvcrt.LK_UNLCK, 1)
            else:
                fcntl.flock(state.fileno(), fcntl.LOCK_UN)


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--image", action="append", default=[], help="Image repository whose latest version is a lower bound")
    parser.add_argument("--version", help="Explicit build version; must be newer than local and published versions")
    parser.add_argument("--check", help="Check a version against published latest images without allocating a version")
    parser.add_argument("--state-file", type=Path, default=Path(__file__).resolve().parent.parent / ".docker-build-version")
    args = parser.parse_args()
    previous = [version for image in args.image for version in image_versions(image)]
    if args.check:
        ensure_newer(args.check, previous)
        return
    with locked_state(args.state_file) as state:
        state.seek(0)
        local = state.read().decode("ascii").strip()
        if local:
            previous.append(local)
        version = args.version or next_version(datetime.now(timezone.utc), previous)
        ensure_newer(version, previous)
        state.seek(0)
        state.truncate()
        state.write(version.encode("ascii"))
        state.flush()
    print(version)


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, subprocess.CalledProcessError) as error:
        # Do not silently fall back to a lower version on registry/network/auth failures.
        print(f"Docker version error: {error}", file=sys.stderr)
        sys.exit(1)
