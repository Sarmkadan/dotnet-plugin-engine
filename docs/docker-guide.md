# Docker Guide
## Quick Start with Docker
1. Build the Docker image:
   ```bash
   docker build -t plugin-engine .
   ```
2. Run the container:
   ```bash
   docker run -d -p 8080:80 plugin-engine
   ```
## Docker Compose Usage
Create a `docker-compose.yml` file:
```yaml
version: '3.8'
services:
  plugin-engine:
    image: plugin-engine
    ports:
      - "8080:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Container
    volumes:
      - ./plugins:/app/plugins
```
Run with Docker Compose:
```bash
docker-compose up -d
```
## Environment Variables
| Variable | Description | Default |
|----------|-------------|---------|
| ASPNETCORE_ENVIRONMENT | Hosting environment | Production |
| PLUGIN_PATH | Path to plugin directory | /app/plugins |
## Production Deployment Checklist
- [ ] Set up resource limits in Docker
- [ ] Configure health checks
- [ ] Set up monitoring and logging
- [ ] Configure SSL/TLS for secure communication
- [ ] Set up backup and disaster recovery