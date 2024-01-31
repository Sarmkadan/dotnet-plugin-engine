# Security Policy

## Supported Versions

| Version | Support Status |
|---------|----------------|
| v2.0.x | ✅ Full support including security updates |
| v1.x    | ⚠️ Security fixes only |

## Reporting a Vulnerability

If you discover a security vulnerability in this project, please report it via email to:

**Email:** [rutova2@gmail.com](mailto:rutova2@gmail.com)

Please include:
- A detailed description of the vulnerability
- Steps to reproduce or proof of concept
- Any potential impact assessment
- Your contact information

We will respond within 5 business days and coordinate disclosure according to coordinated vulnerability disclosure principles.

## Security Update Process

1. **Acknowledgment:** We will confirm receipt of your report within 5 business days
2. **Investigation:** Our security team will investigate the reported issue
3. **Fix Development:** We will develop a fix for the vulnerability
4. **Notification:** We will notify you when a fix is available
5. **Disclosure:** After the fix is released, we will publish a security advisory

## Disclosure Policy

- We follow coordinated vulnerability disclosure
- We will work with you to determine an appropriate disclosure timeline
- We will credit reporters for discovered vulnerabilities (unless anonymity is requested)
- Security fixes will be released as patch versions for supported branches

## Security Best Practices

- Always use the latest supported version in production
- Enable strict input validation for all plugin inputs
- Use AssemblyLoadContext isolation for plugin execution
- Monitor plugin execution for suspicious activity
- Keep dependencies updated via Dependabot alerts