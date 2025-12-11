# Security Policy

## Reporting a Vulnerability

**DO NOT** open a public GitHub issue for security vulnerabilities. This helps prevent exploitation of security issues.

### Private Vulnerability Reporting

We take security seriously and appreciate responsible disclosure. Please report security vulnerabilities using one of these methods:

#### Option 1: GitHub Private Vulnerability Reporting (Recommended)

GitHub provides a dedicated private vulnerability reporting feature:

1. Visit the [GitHub Security Advisories](https://github.com/sarmkadan/dotnet-plugin-engine/security/advisories/new) page
2. Click "Report a vulnerability"
3. Fill in the vulnerability details
4. Submit the report

GitHub will notify the maintainers privately, and you can communicate with us securely within the advisory.

#### Option 2: Email Report

Send vulnerability details to: **rutova2@gmail.com**

Include:
- Description of the vulnerability
- Steps to reproduce (if applicable)
- Potential impact
- Suggested fix (if you have one)

### Response Timeline

- **Acknowledgment**: Within 48 hours
- **Initial Assessment**: Within 1 week
- **Fix & Release**: Timeframe depends on severity
- **Public Disclosure**: Coordinated after fix is available

## Supported Versions

Currently, we provide security updates for:

| Version | Supported | End of Life |
|---------|-----------|-------------|
| 1.x     | ✅ Yes    | TBD        |

Versions not listed are no longer supported and will not receive security updates.

## Security Considerations

### For Users

1. **Keep Updated**: Always use the latest version of dotnet-plugin-engine
   ```bash
   dotnet package update DotnetPluginEngine
   ```

2. **Plugin Loading**: Only load plugins from trusted sources
   - Verify plugin checksums when downloading
   - Review plugin source code before loading
   - Consider plugin isolation and sandboxing

3. **Dependency Management**: Monitor plugin dependencies
   - Review transitive dependencies
   - Keep all dependencies up to date
   - Use dependency scanning tools

4. **Permission Model**: Be aware of what permissions plugins receive
   - Plugins loaded via AssemblyLoadContext have isolation
   - Plugins can still access shared resources
   - Configure appropriate restrictions

5. **Logging & Monitoring**: Enable logging for diagnostics
   - Monitor plugin load failures
   - Track plugin lifecycle events
   - Set up alerts for anomalies

### For Contributors

When implementing security features:

- Follow principle of least privilege
- Validate all inputs at system boundaries
- Use parameterized approaches (avoid string concatenation in sensitive areas)
- Include security-related tests
- Document security assumptions
- Be mindful of timing attacks
- Keep dependencies updated

## Known Limitations

### Current Scope

- Plugin isolation via AssemblyLoadContext provides namespace isolation, not process isolation
- Plugins share the same AppDomain and memory space with the host application
- Plugins have access to shared resources (file system, network, etc.)
- No built-in sandbox or permission system beyond AssemblyLoadContext

### Future Security Work

- Potential process-level isolation using separate AppDomains or processes
- Plugin permission/capability declarations
- Secure plugin signing and verification
- Enhanced audit logging

## Dependency Management

This project depends on several external packages. Security updates to critical dependencies are applied promptly.

To check for vulnerable dependencies:
```bash
# Using dotnet-outdated
dotnet outdated

# Using OWASP DependencyCheck
dotnet add package DependencyCheck
```

## Security Best Practices

### Host Application

```csharp
// Enable logging for diagnostics
services.AddPluginEngine(options =>
{
    options.EnableLogging = true;
    options.MinimumLogLevel = LogLevel.Debug;
    
    // Set reasonable timeouts
    options.OperationTimeoutMs = 30000;
    
    // Validate plugin sources
    options.PluginDirectory = Path.Combine(AppContext.BaseDirectory, "plugins");
});

// Monitor plugin load failures
try
{
    await engine.LoadAllPluginsAsync();
}
catch (PluginLoadException ex)
{
    // Log and handle appropriately
    logger.LogError(ex, "Plugin load failed");
}
```

### Plugin Development

```csharp
// Validate all inputs
public class MyPlugin
{
    public void Process(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Input required");
        
        // Sanitize and process
    }
}

// Use appropriate exception handling
try
{
    // Plugin work
}
catch (Exception ex)
{
    // Log appropriately without exposing sensitive data
    logger.LogError("Processing failed: {Message}", ex.Message);
}
```

## Security-Related Documentation

- [Getting Started Guide](docs/getting-started.md) - Safe plugin loading
- [Architecture Guide](docs/architecture.md) - Isolation model details
- [Deployment Guide](docs/deployment.md) - Production setup
- [FAQ](docs/faq.md) - Security-related FAQs

## Third-Party Security Tools

We support scanning with common security tools:

- **NuGet vulnerability scanning**: Built into Visual Studio and `dotnet` CLI
- **GitHub Security**: Dependabot alerts and code scanning
- **SonarQube**: Code quality and security analysis
- **OWASP DependencyCheck**: Dependency vulnerability detection

## Vulnerability Disclosure

Once a vulnerability is fixed and patched:

1. We will create a security advisory
2. The fix will be released in a new version
3. Public disclosure will be coordinated
4. Release notes will include security details

## Acknowledgments

We appreciate the security research community and responsible disclosure by security researchers.

## Policy Updates

This security policy may be updated periodically. Check back regularly for changes.

---

**Last Updated**: May 2026

For questions about this security policy, please contact the maintainers through the GitHub repository.
