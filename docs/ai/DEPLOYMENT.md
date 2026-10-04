# Docker build versions

Docker tags now use a shared UTC build number, rather than independent local,
GitHub, and GitLab counters:

```text
1.2.<days since 2026-06-01 UTC>.<milliseconds since UTC midnight>
```

For example, a build at `2026-10-04T12:34:56.789Z` produces
`1.2.125.45296789`. Numeric version comparison makes this newer than legacy
`1.0.*` and `1.1.*` tags. Compare version components numerically, never by plain
alphabetical sorting. UTC avoids differences between server and developer time zones.

## Local publishing

Requirements: Python 3, Docker with Buildx, Git, and access to the existing registry.
Linux CI runners need `python3`; no Python packages are required.

```powershell
./deploy.ps1                 # GoldEx and Karat, same build number
./deploy.ps1 goldex          # GoldEx only
./deploy.ps1 karat           # Karat only
```

The script logs into `reg.goldexsoft.ir`, inspects selected `latest` images, and
allocates a version later than both their version labels and the last local build.
The local allocation is stored in ignored `.docker-build-version`, under a file
lock. Allocations are consumed even if the build fails. There is no tracked
`.version` counter to commit or reconcile after CI publishing.

An explicit `-CustomVersion` still works, but must be numeric, use the `1.2`
family, and be strictly newer than all known versions. Normal publishing should
use automatic allocation. If a published image uses a family newer than `1.2`,
update the allocator's `PREFIX` before publishing again.

After publishing, the script prints the exact command to run on the server:

```sh
/home/user/docker/goldex/refresh-apps.sh <generated-version>
```

That server script is external to this repository; no server configuration or
live deployment is changed by this implementation.

## CI and latest

GitHub Actions and GitLab CI call the same Python allocator. Reruns receive a
fresh build number; `github.run_number` and `CI_PIPELINE_IID` are no longer used
as Docker versions. Each CI platform serializes its production publishing jobs.

Both Dockerfiles store the version in `org.opencontainers.image.version` and
`ir.goldex.build.version`, and the source commit in `org.opencontainers.image.revision`.
Only the dedicated `ir.goldex.build.version` marker is used for comparison:
legacy images inherit Ubuntu's OCI version (`24.04`), which is not a GoldEx release.
The allocator reads
remote image configs using Docker's existing registry credentials. Unlabeled
legacy images are accepted for the initial migration; registry authentication,
network, or malformed version errors stop publication instead of guessing.

Numbered image tags are pushed before `latest`. After the builds complete, the
published labels are checked again; if a newer or equal build has reached
`latest`, the older build fails without promoting its images or running the CI
deployment step. Both numbered images must be pushed successfully before either
`latest` alias is promoted.

This follows the accepted assumption that local and CI publishing do not overlap.
The check and registry writes are separate operations: concurrent publishers on
different machines still require a shared server lock for a strict guarantee.
The two `latest` writes also are not an atomic pair. Avoid concurrent publishing
and deploy both applications using the exact generated tag, not a moving alias.
Versions identify build order, not source freshness: intentionally building an
older checkout creates a new build number. Directly deploying an old tag remains
a possible deliberate rollback outside these scripts.

## Application release notes

`src/App/Server/GoldEx.Server/releases.json` retains its existing semantic
versions and Persian user-facing descriptions. Docker build versions do not
replace that history, change accounting behavior, or alter assembly versions.

## Verification

```sh
python3 scripts/test_docker_version.py
pwsh -NoProfile -File scripts/test_deploy.ps1
```

These tests exercise time-zone equivalence, legacy-version migration, repeated
builds, clock rollback, midnight rollover, stale promotion rejection, and registry
config parsing without building or publishing any real images. The PowerShell
tests also check single-application selection and ensure failed builds, failed
pushes, and stale builds never promote `latest`.
