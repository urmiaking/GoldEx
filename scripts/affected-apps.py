"""Select Docker applications from transitive project references and deployed Git revisions."""

import argparse
import fnmatch
import json
import os
from pathlib import Path, PurePosixPath
import posixpath
import re
import subprocess
import sys
import tempfile
import xml.etree.ElementTree as ET


APPLICATIONS = {
    "goldex": "src/App/Server/GoldEx.Server/GoldEx.Server.csproj",
    "calculator": "src/Calculator/Server/GoldEx.Calculator.Server/GoldEx.Calculator.Server.csproj",
}
SHARED_INPUTS = {
    ".dockerignore", "global.json", "nuget.config",
    ".github/workflows/docker-image.yml", "scripts/affected-apps.py",
    "scripts/deploy-affected.py", "scripts/docker-version.py",
}


def git(root, *args):
    return subprocess.run(
        ["git", "-C", str(root), *args], check=True, capture_output=True,
    ).stdout


def read_state(path):
    if not path.exists():
        return {}
    state = json.loads(path.read_text(encoding="utf-8-sig"))
    if not isinstance(state, dict) or any(
        key not in APPLICATIONS or not isinstance(value, str)
        or not re.fullmatch(r"[0-9a-f]{40,64}", value)
        for key, value in state.items()
    ):
        raise ValueError("Invalid deployment state; expected application-to-commit mapping.")
    return state


def record_deployment(path, head, applications):
    state = read_state(path)
    state.update({app: head for app in applications})
    path.parent.mkdir(parents=True, exist_ok=True)
    # This file is outside checkout. Production workflow concurrency serializes writes.
    with tempfile.NamedTemporaryFile(mode="w", encoding="utf-8", dir=path.parent, delete=False) as stream:
        temporary = Path(stream.name)
        json.dump(state, stream, indent=2)
        stream.write("\n")
    try:
        os.replace(temporary, path)
    finally:
        temporary.unlink(missing_ok=True)


class Snapshot:
    def __init__(self, root, revision):
        self.root = root
        self.revision = revision
        paths = git(root, "ls-tree", "-r", "--name-only", "-z", revision).decode("utf-8").split("\0")
        self.projects = {path for path in paths if path.endswith(".csproj")}
        self.build_imports = {
            path for path in paths if PurePosixPath(path).name in {
                "Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props",
            }
        }
        self.cache = {}

    def project(self, path):
        if path not in self.cache:
            if path not in self.projects:
                raise ValueError(f"Missing referenced project: {path}")
            if any(str(parent / name) in self.build_imports
                   for parent in PurePosixPath(path).parents
                   for name in ("Directory.Build.props", "Directory.Build.targets", "Directory.Packages.props")):
                # These may inject extra references/properties. Do not pretend XML is MSBuild.
                raise ValueError(f"Implicit MSBuild imports require conservative detection: {path}")
            tree = ET.fromstring(git(self.root, "show", f"{self.revision}:{path}"))
            references, inputs = set(), set()
            for node in tree.iter():
                tag = node.tag.rsplit("}", 1)[-1]
                if tag not in {"ProjectReference", "Import", "Compile", "Content", "None", "EmbeddedResource"}:
                    continue
                if tag == "Import":
                    raise ValueError(f"MSBuild import requires conservative detection: {path}")
                value = node.get("Project") if tag == "Import" else node.get("Include")
                if not value:
                    continue
                for item in value.split(";"):
                    # Conditions are deliberately included: any possible dependency is relevant.
                    if "$" in item or "@(" in item or "%(" in item:
                        raise ValueError(f"Dynamic MSBuild input in {path}: {item}")
                    normalized = posixpath.normpath(posixpath.join(posixpath.dirname(path), item.replace("\\", "/")))
                    if normalized.startswith("../") or normalized.startswith("/"):
                        raise ValueError(f"Input outside repository in {path}")
                    if tag == "ProjectReference":
                        if any(character in normalized for character in "*?["):
                            raise ValueError(f"Wildcard project reference in {path}")
                        references.add(normalized)
                    else:
                        inputs.add(normalized)
            self.cache[path] = references, inputs
        return self.cache[path]

    def dependencies(self, entrypoint):
        visited, pending, inputs = set(), [entrypoint], set()
        while pending:
            project = pending.pop()
            if project in visited:
                continue
            visited.add(project)
            references, linked_inputs = self.project(project)
            pending.extend(references - visited)
            inputs.update(linked_inputs)
        return visited, inputs

    def affects(self, app, changed):
        dependencies, inputs = self.dependencies(APPLICATIONS[app])
        directories = {}
        for project in self.projects:
            directories.setdefault(posixpath.dirname(project), set()).add(project)
        for path in changed:
            if ignored(path):
                continue
            name = PurePosixPath(path).name.lower()
            if path.lower() in SHARED_INPUTS or path.startswith("nuget/") or name.endswith((".props", ".targets")):
                return path
            if path.startswith("fonts/"):
                if app == "goldex":
                    return path
                continue
            if any(fnmatch.fnmatchcase(path, pattern) for pattern in inputs):
                return path
            owners = [directory for directory in directories if path.startswith(directory + "/")]
            if owners:
                owners = directories[max(owners, key=len)]
                if owners & dependencies:
                    return path
                continue
            # Unknown build/runtime inputs are conservative; never silently skip a deployment.
            return path
        return None


def ignored(path):
    name = PurePosixPath(path).name.lower()
    return (
        path.startswith(("docs/", "src/Tests/", "scripts/tests/"))
        or name in {"agents.md", "readme.md", "license.txt", "license", ".gitignore", ".gitattributes"}
        or name.endswith((".md", ".sln", ".slnx", ".dotsettings", ".user"))
        or path.startswith("scripts/test_")
        or path in {".gitlab-ci.yml", "deploy.ps1", "dotnet-tools.json"}
    )


def detect(root, head, state, force=()):
    result = {}
    snapshots = {}
    for app in APPLICATIONS:
        if app in force:
            result[app] = "Explicit application selection"
            continue
        base = state.get(app)
        if not base:
            result[app] = "No successful deployment recorded (initial full deployment)"
            continue
        try:
            # Diverged or missing history is unsafe to filter (force pushes/old release tags).
            git(root, "merge-base", "--is-ancestor", base, head)
            changed = git(root, "diff", "--no-renames", "--name-only", "-z", base, head).decode("utf-8").split("\0")
            changed = [path for path in changed if path and not ignored(path)]
            for revision in (head, base):
                if not changed:
                    break
                if revision not in snapshots:
                    snapshots[revision] = Snapshot(root, revision)
                reason = snapshots[revision].affects(app, changed)
                if reason:
                    result[app] = f"Affected input: {reason} (since {base[:12]})"
                    break
        except (subprocess.CalledProcessError, ValueError, ET.ParseError) as error:
            result[app] = f"Conservative rebuild: history/dependency graph unavailable ({type(error).__name__})"
    return result


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--root", type=Path, default=Path(__file__).resolve().parent.parent)
    parser.add_argument("--state-file", type=Path, required=True)
    parser.add_argument("--target", choices=["auto", "both", *APPLICATIONS], default="auto")
    parser.add_argument("--event", default="push")
    parser.add_argument("--record", action="store_true", help="Record only after deployment and Nginx reload succeed")
    parser.add_argument("--app", choices=list(APPLICATIONS), action="append", default=[])
    args = parser.parse_args()
    head = git(args.root, "rev-parse", "HEAD").decode("ascii").strip()
    if args.record:
        if not args.app:
            parser.error("--record requires at least one --app")
        record_deployment(args.state_file, head, args.app)
        return
    forced = list(APPLICATIONS) if args.target == "both" or args.event == "release" else (
        [args.target] if args.target != "auto" else []
    )
    # A manual single-app run is an explicit selection, not auto plus a forced app.
    affected = detect(args.root, head, read_state(args.state_file), forced)
    if args.target in APPLICATIONS and args.event != "release":
        affected = {args.target: affected[args.target]}
    outputs = {app: str(app in affected).lower() for app in APPLICATIONS}
    outputs["any"] = str(bool(affected)).lower()
    if os.environ.get("GITHUB_OUTPUT"):
        with open(os.environ["GITHUB_OUTPUT"], "a", encoding="utf-8") as stream:
            stream.writelines(f"{key}={value}\n" for key, value in outputs.items())
    summary = "## Affected applications\n\n" + "\n".join(
        f"- **{app}**: {affected.get(app, 'Unchanged; build and deployment skipped')}" for app in APPLICATIONS
    ) + "\n"
    if os.environ.get("GITHUB_STEP_SUMMARY"):
        with open(os.environ["GITHUB_STEP_SUMMARY"], "a", encoding="utf-8") as stream:
            stream.write(summary)
    print(summary)


if __name__ == "__main__":
    try:
        main()
    except (ValueError, OSError, subprocess.CalledProcessError) as error:
        print(f"Affected application detection failed: {error}", file=sys.stderr)
        sys.exit(1)
