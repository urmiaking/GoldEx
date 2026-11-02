# GoldEx deployment readiness and rollback

Use this skill for changes to GitHub Docker deployment, health probes, production Compose overlays, database readiness, Nginx reload, and rollback.

## Contracts

- GitHub Actions `.github/workflows/docker-image.yml` deploys only affected `goldex` (all tenants) and/or `calculator`, detected from Git history; it must not deploy untouched applications.
- Read `docs/ai/DEPLOYMENT.md`. Server configuration is the live `/home/user/docker/goldex/docker-compose.yaml` and its `.env`; never replace it with a sample or accidentally log secrets.
- Both servers expose an early unauthenticated `/health/ready`, bypassing tenant, license and HTTPS redirect. Return 200 **only** when the app is serving and the SQL database actually executes a query; return 503 on failure. No external price providers in this release gate.
- Runtime Dockerfiles include `curl` and label `ir.goldex.health.ready=true`. Temporary Compose override attaches an HTTP healthcheck to selected services and requires exactly 200; `docker compose up --wait` alone does not prove readiness without that healthcheck.
- Before any recreation, capture **all** selected services' old immutable image IDs. The deployment script must restore all of them after partial updates, unhealthy status, or Nginx failure. For old images without the new health support, disable the newly added probe during rollback, verify at least that the prior image runs, and warn that this is weaker than the new gate.
- Validate and gracefully reload Nginx *inside the rollback boundary*. Unselected services, DB, uploads, mounts, and the existing `.env` must remain untouched. Do not advance GitHub successful-deployment state on failure. Stop failed first-time services.
- Keep Docker tags immutable and promote remote latest aliases only **after** successful readiness; do not automatically release or push to GitHub. Registry-alias and CI state errors after readiness are outside the script's rollback boundary.
- GoldEx applies EF migrations on startup. The image rollback is not a SQL-schema rollback; incompatible schema migrations require separate release safeguards.

## Verification and delivery

Run `python scripts/test_deploy_affected.py` (includes offline simulated DB failure, partial restart and rollback, plus Docker Compose overlay compatibility where available), `python scripts/test_affected_apps.py`, `python scripts/test_docker_version.py`, `git diff --check`, and compile the two server projects for C# changes. A real deployment and database failure remain untested until an explicitly authorized staging/release window.

2026-11-02 user specifically requested **no version bump, push, merge or deployment**. Keep changes in local `Dev` and commit only task-owned files, excluding concurrent unrelated work.
