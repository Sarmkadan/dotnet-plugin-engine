# Migration Guide: v1.x to v2.0

This guide covers breaking changes and required steps to upgrade from dotnet-plugin-engine v1.x to v2.0.

## Breaking Changes

### 1. Default Port Changed: 5000 -> 8080

The default HTTP port has been changed from `5000` to `8080` to align with containerized deployment conventions and avoid conflicts with common development ports.

**Action required:**
- Update any reverse proxy configurations (nginx, Caddy, etc.) to point to port 8080
- Update Docker port mappings if overridden
- Update health check URLs if hardcoded
- Update `ASPNETCORE_URLS` environment variable if set explicitly

```yaml
# Before (v1.x)
ports:
  - "5000:5000"

# After (v2.0)
ports:
  - "8080:8080"
```

### 2. Runtime Base Image: dotnet/runtime -> dotnet/aspnet

The Dockerfile runtime stage now uses `mcr.microsoft.com/dotnet/aspnet:10.0` instead of `mcr.microsoft.com/dotnet/runtime:10.0`. This provides built-in Kestrel support and HTTP pipeline capabilities needed for the health check endpoint.

**Action required:**
- If you have a custom Dockerfile extending the base image, update the `FROM` directive
- Container image size will increase slightly (~30MB) due to ASP.NET runtime inclusion

### 3. Health Check Endpoint

The HEALTHCHECK directive now uses an actual HTTP endpoint (`/health`) instead of `dotnet --version`. This provides meaningful health status rather than just verifying the runtime is installed.

**Action required:**
- Ensure your deployment does not block outbound curl requests within the container
- If using custom health check configurations, update the endpoint URL to include the new port

```dockerfile
# Before (v1.x)
HEALTHCHECK CMD dotnet --version || exit 1

# After (v2.0)
HEALTHCHECK CMD curl -f http://localhost:8080/health || exit 1
```

### 4. Docker Compose Schema

The `version` field has been removed from `docker-compose.yml` as it is deprecated in Docker Compose v2+. The `docker-compose` command is replaced by `docker compose` (without hyphen).

**Action required:**
- Use `docker compose` instead of `docker-compose` in scripts and CI/CD pipelines
- Remove `version` field from any custom compose overrides

### 5. ASPNETCORE_URLS Environment Variable

The `ASPNETCORE_URLS` variable is now explicitly set to `http://+:8080` in both the Dockerfile and docker-compose.yml.

**Action required:**
- Remove any conflicting `ASPNETCORE_URLS` settings in your deployment
- If you need a different port, override this variable in your compose file or environment

## Step-by-Step Upgrade

1. **Update your docker-compose.override.yml** (if any):
   - Change port mappings from 5000 to 8080
   - Remove the `version` field
   - Update health check URLs

2. **Update reverse proxy**:
   ```
   # Caddy example
   your-domain.com {
       reverse_proxy localhost:8080
   }
   ```

3. **Update CI/CD pipelines**:
   - Replace `docker-compose` with `docker compose`
   - Update any port references from 5000 to 8080
   - Update health check probe URLs

4. **Update monitoring**:
   - Update Prometheus/Grafana scrape targets if applicable
   - Update uptime monitoring URLs

5. **Rebuild and deploy**:
   ```bash
   docker compose build --no-cache
   docker compose up -d
   ```

## New Features in v2.0

- Multi-stage Docker build with test execution during build
- Proper HTTP-based health checks via `/health` endpoint
- ASP.NET runtime for full Kestrel HTTP pipeline support
- Non-root container user (`appuser`, UID 1000)
- Structured logging with JSON file driver and rotation
- Resource limits and reservations in compose
- Plugin file system monitor as a separate service

## Rollback

If you need to rollback to v1.x:

1. Check out the v1.x tag: `git checkout v1.0.0`
2. Rebuild: `docker compose build --no-cache`
3. Restart: `docker compose up -d`

## Questions

For migration issues, open a [GitHub Issue](https://github.com/Sarmkadan/dotnet-plugin-engine/issues) with the `migration` label.
