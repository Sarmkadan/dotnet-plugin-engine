# Deployment Guide

This guide covers deploying dotnet-plugin-engine in production environments.

## Pre-Deployment Checklist

- [ ] .NET 10 runtime installed on target environment
- [ ] Plugin directory exists with proper permissions
- [ ] Configuration file reviewed and validated
- [ ] Logging configured and monitored
- [ ] Health check endpoints configured
- [ ] Backup/rollback procedure documented
- [ ] Resource limits determined (memory, CPU)
- [ ] Plugin integrity verification implemented
- [ ] Monitoring and alerting configured

## System Requirements

### Minimum Requirements
- **.NET Runtime 10.0** or later
- **RAM**: 256 MB base + 50 MB per concurrent plugin load
- **Disk**: 100 MB application + plugin directory space
- **CPU**: 1 core minimum

### Recommended Requirements (Production)
- **.NET Runtime 10.0.x** (latest patch)
- **RAM**: 2 GB minimum, 4+ GB for heavy workloads
- **Disk**: SSD with 1+ GB free space for plugins and logs
- **CPU**: 4+ cores for concurrent plugin operations
- **OS**: Windows Server 2022+ or Linux (Ubuntu 22.04+)

## Installation Methods

### Docker Deployment

Create a Dockerfile:

```dockerfile
FROM mcr.microsoft.com/dotnet/runtime:10.0 as runtime

WORKDIR /app

# Copy application
COPY bin/Release/net10.0/publish ./
COPY plugins ./plugins

# Create non-root user
RUN useradd -m -u 1000 appuser && chown -R appuser:appuser /app
USER appuser

EXPOSE 5000
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD dotnet app.dll health

ENTRYPOINT ["dotnet", "app.dll"]
```

Build and run:

```bash
docker build -t my-plugin-app:1.0.0 .
docker run --name plugin-app \
    -p 5000:5000 \
    -v plugins:/app/plugins \
    -e ASPNETCORE_ENVIRONMENT=Production \
    my-plugin-app:1.0.0
```

### Docker Compose Deployment

```yaml
version: '3.8'

services:
  app:
    build: .
    container_name: plugin-app
    ports:
      - "5000:5000"
    volumes:
      - ./plugins:/app/plugins
      - ./logs:/app/logs
    environment:
      ASPNETCORE_ENVIRONMENT: Production
      LOG_LEVEL: Information
    healthcheck:
      test: ["CMD", "dotnet", "app.dll", "health"]
      interval: 30s
      timeout: 10s
      retries: 3
    restart: unless-stopped
    networks:
      - plugin-network

networks:
  plugin-network:
    driver: bridge
```

### Direct Installation

```bash
# On Windows
dotnet publish -c Release -o C:\PluginApp
cd C:\PluginApp
mkdir plugins
dotnet PluginApp.dll

# On Linux
dotnet publish -c Release -o ~/plugin-app
cd ~/plugin-app
mkdir plugins
chmod +x plugin-app
./plugin-app
```

## Configuration for Production

### appsettings.json

```json
{
  "PluginEngine": {
    "PluginDirectory": "/app/plugins",
    "EnableHotReload": true,
    "HotReloadCheckIntervalMs": 10000,
    "EnableDependencyCaching": true,
    "DependencyCacheTTLMs": 600000,
    "OperationTimeoutMs": 60000,
    "MaxConcurrentPluginLoads": 4,
    "StrictVersionChecking": true,
    "EnableCircularDependencyDetection": true,
    "MaxDependencyResolutionAttempts": 15,
    "EnableLogging": true,
    "MinimumLogLevel": "Information",
    "WebhookConfig": {
      "Enabled": true,
      "BaseUrl": "https://myapp.example.com/webhooks",
      "Events": ["plugin.loaded", "plugin.failed", "plugin.reloaded"],
      "RetryAttempts": 3,
      "RetryDelayMs": 5000
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning",
      "PluginEngine": "Debug"
    },
    "Console": {
      "IncludeScopes": true
    },
    "File": {
      "Path": "/app/logs/plugin-engine.log",
      "RollingInterval": "Day",
      "FileSizeLimit": 104857600
    }
  }
}
```

### Environment Variables

```bash
export DOTNET_ENVIRONMENT=Production
export ASPNETCORE_ENVIRONMENT=Production
export LOG_LEVEL=Information
export PLUGIN_DIRECTORY=/app/plugins
export OPERATION_TIMEOUT_MS=60000
export HOT_RELOAD_ENABLED=true
```

## Monitoring & Observability

### Health Checks

Implement health endpoint:

```csharp
app.MapGet("/health", async (PluginEngine engine) =>
{
    var health = await engine.GetHealthInfoAsync();
    return Results.Ok(new
    {
        status = health.IsHealthy ? "healthy" : "degraded",
        loadedPlugins = health.LoadedPluginsCount,
        failedPlugins = health.FailedPluginsCount,
        avgLoadTimeMs = health.AveragePluginLoadTimeMs,
        timestamp = DateTime.UtcNow
    });
});

app.MapGet("/health/ready", async (PluginEngine engine) =>
{
    var health = await engine.GetHealthInfoAsync();
    return health.IsHealthy
        ? Results.Ok("Ready")
        : Results.StatusCode(503);
});
```

### Metrics & Logging

Configure structured logging:

```csharp
builder.Services.AddLogging(config =>
{
    config
        .AddConsole()
        .AddFile("/app/logs/plugin-engine.log", rollingInterval: RollingInterval.Day)
        .SetMinimumLevel(LogLevel.Information);
});
```

Log plugin operations:

```csharp
logger.LogInformation("Plugin {PluginName} v{Version} loaded successfully",
    plugin.Name, plugin.Version);

logger.LogWarning("Dependency resolution failed for {PluginId}: {@Violations}",
    pluginId, violations);

logger.LogError("Plugin {PluginName} hot reload failed: {Error}",
    pluginName, errorMessage);
```

### Prometheus Metrics (Optional)

```csharp
var counter = new Counter("plugin_loads_total", "Total plugin loads");
var histogram = new Histogram("plugin_load_duration_ms", "Plugin load duration");

using (histogram.NewTimer())
{
    var plugin = await loader.LoadPluginAsync(path);
    counter.Inc();
}
```

## Performance Tuning

### Memory Optimization

```csharp
services.AddPluginEngine(options =>
{
    // Adjust based on available memory
    options.MaxConcurrentPluginLoads = Environment.ProcessorCount / 2;
    
    // Shorter TTL for memory-constrained environments
    options.DependencyCacheTTLMs = 120000; // 2 minutes
    
    // Disable hot reload if memory is critical
    options.EnableHotReload = false;
});
```

### CPU Optimization

```csharp
// Reduce concurrent loads if CPU-bound
options.MaxConcurrentPluginLoads = 2;

// Increase hot reload interval to reduce I/O
options.HotReloadCheckIntervalMs = 15000;

// Longer cache TTL to reduce resolution overhead
options.DependencyCacheTTLMs = 900000; // 15 minutes
```

### Disk I/O Optimization

```csharp
// Use SSD for plugin directory
// Enable caching
options.EnableDependencyCaching = true;

// Batch hot reload checks
options.HotReloadCheckIntervalMs = 10000;

// Consider disabling hot reload for read-only deployments
options.EnableHotReload = environment == "Production" ? false : true;
```

## Security Considerations

### Plugin Directory Permissions

```bash
# Linux/macOS
sudo chown -R app:app /app/plugins
sudo chmod 750 /app/plugins
sudo chmod 640 /app/plugins/*.dll

# Windows
icacls "C:\PluginApp\plugins" /grant "APPUSER:(OI)(CI)F" /T
```

### Assembly Verification

Implement signature verification:

```csharp
private bool VerifyPluginSignature(string assemblyPath)
{
    var certificate = X509Certificate.CreateFromSignedFile(assemblyPath);
    
    // Verify certificate thumbprint matches known good value
    return certificate.GetCertHashString() == "EXPECTED_THUMBPRINT";
}
```

### Dependency Scanning

Before loading plugins, scan for known vulnerabilities:

```csharp
private async Task<bool> ScanForVulnerabilities(Plugin plugin)
{
    // Check against vulnerability database
    var vulnerabilities = await VulnerabilityScanner.ScanAsync(plugin);
    return !vulnerabilities.Any(v => v.IsCritical);
}
```

### Network Security

If using webhook integration:

```yaml
WebhookConfiguration:
  Enabled: true
  BaseUrl: "https://secure-endpoint.example.com"
  UseHttps: true
  ValidateCertificate: true
  ApiKey: "${WEBHOOK_API_KEY}"  # Use secrets management
```

## Backup & Recovery

### Plugin Backup Strategy

```bash
# Daily backup
0 2 * * * tar -czf /backup/plugins-$(date +\%Y\%m\%d).tar.gz /app/plugins

# Keep 30 days of backups
find /backup -name "plugins-*.tar.gz" -mtime +30 -delete
```

### Recovery Procedure

```bash
# Stop application
systemctl stop plugin-app

# Restore from backup
tar -xzf /backup/plugins-20250504.tar.gz -C /app

# Verify plugins
dotnet app.dll verify-plugins

# Start application
systemctl start plugin-app
```

## Rollout Strategy

### Canary Deployment

Deploy to small subset first:

```bash
# Stage 1: Canary (10% traffic)
# Deploy to 1 of 10 instances

# Stage 2: Gradual rollout
# Deploy to 5 instances, monitor for 24 hours

# Stage 3: Full deployment
# Deploy to remaining instances
```

### Blue-Green Deployment

```bash
# Blue environment (current)
docker-compose -f docker-compose.blue.yml up

# Green environment (new version)
docker-compose -f docker-compose.green.yml up

# After verification, switch traffic to green
# Keep blue running for rollback
```

### Zero-Downtime Hot Reload

Plugin updates don't require application restart:

```csharp
// Copy new plugin version
File.Copy("plugin-v2.0.dll", "plugins/plugin.dll", overwrite: true);

// Engine detects change and reloads automatically
// No downtime, old version unloaded gracefully
```

## Troubleshooting Production Issues

### Plugin Load Failure

```bash
# Check logs
tail -f /app/logs/plugin-engine.log | grep ERROR

# Verify plugin file
file /app/plugins/myplugin.dll

# Check .NET version compatibility
dotnet --info
```

### Memory Leak Detection

```bash
# Monitor memory usage
watch -n 1 'ps aux | grep plugin-app | grep -v grep'

# Enable detailed logging
DOTNET_Diagnostic_DiagnosticPort=/tmp/dotnet-diagnostic.sock
dotnet-dump collect -p <PID>
```

### High CPU Usage

Check for circular dependencies or expensive operations:

```csharp
var graph = await dependencyResolver.GetDependencyGraphAsync(pluginId);
// Analyze graph for issues

var stats = await hotReloader.GetStatisticsAsync();
// Check reload frequency
```

## Capacity Planning

### Estimation

- **Per plugin**: 5-50 MB RAM (varies by complexity)
- **Per 1000 dependencies**: ~5 MB cache
- **Per second**: ~100 plugin operations

### Scaling

For high load:
- Use multiple instances with load balancing
- Share plugin directory via network storage
- Implement plugin caching layer
- Consider plugin compilation/pre-loading

## Disaster Recovery

### RTO/RPO Targets

- **RTO** (Recovery Time Objective): 5 minutes
- **RPO** (Recovery Point Objective): 1 hour

### Failover Checklist

1. Keep backup plugin directory synchronized
2. Store configuration in version control
3. Document plugin dependency tree
4. Maintain list of critical plugins
5. Have rollback procedure documented

## Post-Deployment

### Monitoring

Set up alerts for:
- Plugin load failures
- Memory usage > 80%
- CPU usage > 90%
- Failed hot reloads
- Circular dependency detection

### Optimization

- Monitor metrics for 1 week
- Adjust concurrency settings
- Fine-tune cache TTL
- Optimize hot reload interval

### Documentation

Document:
- Deployed plugin versions
- Configuration customizations
- Known limitations
- Disaster recovery procedures
- Escalation contacts
