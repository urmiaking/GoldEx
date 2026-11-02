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
deployment step. All selected numbered images must be pushed successfully before
any selected `latest` alias is promoted. GitHub can select one application; GitLab
continues publishing both.

This follows the accepted assumption that local and CI publishing do not overlap.
The check and registry writes are separate operations: concurrent publishers on
different machines still require a shared server lock for a strict guarantee.
The two `latest` writes also are not an atomic pair. Avoid concurrent publishing
and deploy both applications using the exact generated tag, not a moving alias.
Versions identify build order, not source freshness: intentionally building an
older checkout creates a new build number. Directly deploying an old tag remains
a possible deliberate rollback outside these scripts.

## Nginx reload after CI deployment

After successful deployment, both CI pipelines run these commands against the
existing Nginx container on the runner's Docker host:

```sh
docker exec "$NGINX_CONTAINER" nginx -t
docker exec "$NGINX_CONTAINER" nginx -s reload
```

The default container name is `nginx`. GitHub Actions accepts an optional
`NGINX_CONTAINER` repository variable; GitLab accepts a CI/CD variable with the
same name. The runner must have Docker access to the host running this container.

GitHub runs this step after tracked `scripts/deploy-affected.py` completes its
selected Compose update and waits for the selected services to be running/healthy
(up to 180 seconds; health readiness requires a healthcheck in Compose).
GitLab runs it after external `/masoud/initialize.sh` returns success; that script
must finish replacing the application containers before returning and return
nonzero on failure. GitHub no longer calls the external all-app `refresh-apps.sh`.

A failed deployment skips the reload. A missing/stopped Nginx container or an
invalid configuration fails the job before sending the reload signal. A failed
reload command also fails the job. Reload is graceful and does not restart the
container. Local `deploy.ps1` only publishes images and is unaffected.

## Application release notes

`src/App/Server/GoldEx.Server/releases.json` retains its existing semantic
versions and Persian user-facing descriptions. Docker build versions do not
replace that history, change accounting behavior, or alter assembly versions.

## GitHub: deploy only affected applications

The workflow resolves transitive `ProjectReference` dependencies from the two
server entrypoints without restoring or building .NET. It reads Git snapshots at
both ends of each comparison, includes conditional references conservatively,
and treats renames as deletion plus addition. A shared project can live anywhere:
Calculator references several `src/App` projects, so `src/App/**` is not an
enterprise-only path filter.

The detector intentionally does not evaluate MSBuild. Dynamic reference paths or
explicit/implicit MSBuild imports cause conservative selection, since imported
files can inject dependencies that a direct XML reference scan cannot prove.

| Changed input | Applications |
| --- | --- |
| `GoldEx.Server`, `GoldEx.Client`, their private assets/Dockerfile | GoldEx |
| Calculator server/client/shared projects and Dockerfile | Calculator (`goldex-karat`) |
| Enterprise Client.Components/Services/Abstractions, Shared, Application/Infrastructure/Domain, SDK | Both |
| `fonts/` (copied only by the enterprise Dockerfile) | GoldEx |
| NuGet packages/config, `.props`/`.targets`, `.dockerignore`, shared workflow/deploy tooling | Both |
| Documentation, test tooling, `src/Tests`, unreferenced project directories | Neither |
| Unknown runtime input or unavailable history/dependency graph | Conservative rebuild |

Each application has a separate last-successful-deployment SHA, in
`$HOME/.local/state/goldex/github-deployments.json`. Checkout cleanup cannot remove
this file. Only selected entries advance, after deployment **and** Nginx reload
succeed. A failed deployment therefore stays pending for the next run. Changes
from pending runs replaced by GitHub's concurrency queue are also included;
using only the current push's `before` SHA would miss those changes.

The first run, a missing state entry, an unavailable commit or diverged history
rebuilds the corresponding app. Invalid state JSON stops the workflow. Keep the
existing production concurrency group. If multiple self-hosted runners target
the same server, set `GOLDEX_DEPLOY_STATE_FILE` to a shared persistent file with
runner write access. If state is lost, both apps safely redeploy. Deployment
through GitLab, local tools or the old refresh script does not update this file;
run manual `both` afterwards to establish a fresh GitHub baseline.

Pushes to `master` use automatic detection. `workflow_dispatch` offers `auto`,
`goldex`, `calculator` and `both`; a specific app is an explicit selection even
when unchanged. Published releases always deploy both. The Actions summary
shows the selection and its reason. If neither app is affected, no Docker version
is allocated, no image is published, no service is updated and Nginx is untouched.

Deployment uses the **existing production Compose file**, not the sample Compose
file in `src/App`. The helper discovers all services whose image repository is
`reg.goldexsoft.ir/goldex` or `reg.goldexsoft.ir/goldex-karat`, including multiple
GoldEx store instances. Missing selected image mappings stop deployment. It
passes `VERSION` as before, pins selected image tags with a temporary JSON Compose
override, pulls every selected image before replacement, and runs:

```sh
docker compose ... up -d --no-deps --no-build --pull never --wait --wait-timeout 180 <selected-services>
```

The existing environment, mounts, networks and tenant service definitions remain
in the production Compose file. Unselected services and dependencies are not
recreated; orphan removal and global pruning are intentionally omitted. The
original GoldEx directory preparation/permission commands run when GoldEx is
selected. The old external refresh script can remain on disk but is no longer
called by GitHub, so it cannot accidentally update both applications.

Optional GitHub repository variables:

| Variable | Default |
| --- | --- |
| `GOLDEX_COMPOSE_FILE` | `/home/user/docker/goldex/docker-compose.yaml` |
| `GOLDEX_APP_BASE` | `/var/volumes/apps/goldex` |
| `GOLDEX_DEPLOY_STATE_FILE` | `$HOME/.local/state/goldex/github-deployments.json` |
| `NGINX_CONTAINER` | `nginx` |

The runner needs Python 3, registry credentials as before, Docker Buildx, Docker
Compose V2 supporting JSON config and `up --wait`, access to the production
Compose file and its environment files, and the same sudo directory permissions
as the previous refresh script. No extra Python packages are needed.

References: [GitHub conditional contexts](https://docs.github.com/en/actions/reference/workflows-and-actions/contexts),
[Compose service selection and `--no-deps`/`--wait`](https://docs.docker.com/reference/cli/docker/compose/up/).

## Verification

```sh
python3 scripts/test_docker_version.py
python3 scripts/test_affected_apps.py
python3 scripts/test_deploy_affected.py
pwsh -NoProfile -File scripts/test_deploy.ps1
```

These tests exercise time-zone equivalence, legacy-version migration, repeated
builds, clock rollback, midnight rollover, stale promotion rejection, and registry
config parsing without building or publishing any real images. The PowerShell
tests also check single-application selection and ensure failed builds, failed
pushes, and stale builds never promote `latest`.

The affected-app tests additionally cover real project references, transitive
shared code, linked files, deletions/renames, docs-only changes, distinct app
baselines, skipped/failed runs, missing or diverged history, explicit selection,
and state recording. Deployment tests mock Docker and sudo to verify selected
services/images, immutable tags and failure propagation without touching a server.
