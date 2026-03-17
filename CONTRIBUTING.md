# Contributing to dotnet-plugin-engine

Thank you for your interest in contributing to the **dotnet-plugin-engine** project! We welcome contributions from the community and appreciate your help in making this project better.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Setup](#development-setup)
- [Making Changes](#making-changes)
- [Code Style](#code-style)
- [Testing](#testing)
- [Committing Changes](#committing-changes)
- [Pull Request Process](#pull-request-process)
- [Reporting Issues](#reporting-issues)
- [License](#license)

## Code of Conduct

Please review our [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md) before contributing. We are committed to providing a welcoming and inclusive environment for all contributors.

## Getting Started

### Forking the Repository

1. Click the **Fork** button on the [GitHub repository](https://github.com/sarmkadan/dotnet-plugin-engine)
2. Clone your fork locally:
   ```bash
   git clone https://github.com/YOUR_USERNAME/dotnet-plugin-engine.git
   cd dotnet-plugin-engine
   ```
3. Add the upstream repository:
   ```bash
   git remote add upstream https://github.com/sarmkadan/dotnet-plugin-engine.git
   ```

### Cloning the Repository

If you already have commit access:

```bash
git clone https://github.com/sarmkadan/dotnet-plugin-engine.git
cd dotnet-plugin-engine
```

## Development Setup

### Prerequisites

- **.NET 10.0 SDK** or later ([download](https://dotnet.microsoft.com/en-us/download))
- **Git** for version control
- A text editor or IDE (Visual Studio, Visual Studio Code, JetBrains Rider, etc.)

### Environment Setup

```bash
# Restore NuGet packages
dotnet restore

# Verify build succeeds
dotnet build

# Run all tests
dotnet test

# Build release package
dotnet pack -c Release
```

### Setting Up Your Branch

1. Create a new branch for your feature or fix:
   ```bash
   git checkout -b feature/your-feature-name
   ```
   or
   ```bash
   git checkout -b fix/issue-description
   ```

2. Branch naming conventions:
   - `feature/description-of-feature` - New features
   - `fix/issue-description` - Bug fixes
   - `docs/description` - Documentation improvements
   - `refactor/description` - Code refactoring
   - `test/description` - Test additions/improvements

## Making Changes

### Guidelines

1. **One concern per PR**: Keep pull requests focused on a single feature or fix
2. **Small and testable**: Aim for changes that can be easily reviewed and tested
3. **Keep main clean**: Ensure your branch is up-to-date with upstream main before submitting
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

4. **Add tests**: Include tests for new functionality or bug fixes
5. **Update documentation**: If your changes affect the public API or user-facing behavior, update relevant documentation

### Project Structure

```
dotnet-plugin-engine/
├── src/PluginEngine/              # Main library code
│   ├── Domain/Entities/           # Core domain models
│   ├── Services/                  # Business logic services
│   ├── Data/Repositories/         # Data access layer
│   ├── Configuration/             # Configuration classes
│   ├── Middleware/                # Plugin execution middleware
│   ├── Events/                    # Event publishing/subscription
│   ├── Integration/               # External integration
│   ├── Utils/                     # Utilities and helpers
│   └── PluginEngine.cs            # Main façade
├── examples/                       # Usage examples
├── docs/                          # Documentation
├── tests/                         # Test projects
└── README.md
```

## Code Style

### C# Conventions

- **Naming**: Follow [C# naming conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
  - `PascalCase` for public members and types
  - `camelCase` for local variables and private fields
  - `_camelCase` for private fields with underscore prefix

- **Formatting**: Use `dotnet format` before committing
  ```bash
  dotnet format
  ```

- **XML Documentation**: Document all public members
  ```csharp
  /// <summary>
  /// Brief description of the method.
  /// </summary>
  /// <param name="parameter">Description of parameter.</param>
  /// <returns>Description of return value.</returns>
  public async Task<ResultType> MethodNameAsync(string parameter)
  {
  }
  ```

- **async/await**: Use async/await for all I/O operations
  - Method names ending in `Async` should return `Task` or `Task<T>`
  - Use `await` when calling async methods

- **Null handling**: Use nullable reference types
  ```csharp
  public string? NullableProperty { get; set; }
  public required string RequiredProperty { get; set; }
  ```

- **Author headers**: Preserve existing author headers in files
  ```csharp
  // =============================================================================
  // Author: Vladyslav Zaiets | https://sarmkadan.com
  // CTO & Software Architect
  // =============================================================================
  ```

### Code Quality

- Avoid compiler warnings - fix all warnings before committing
- Use meaningful variable and method names
- Keep methods focused and reasonably sized
- Prefer composition over inheritance
- Use interfaces for abstraction and dependency injection
- Include appropriate logging at DEBUG level for diagnostics

## Testing

### Running Tests

```bash
# Run all tests
dotnet test

# Run specific test project
dotnet test tests/PluginEngine.Tests

# Run with verbosity
dotnet test -v detailed

# Run with code coverage
dotnet test /p:CollectCoverage=true
```

### Writing Tests

- Write unit tests for new functionality
- Use descriptive test names: `MethodName_Scenario_ExpectedResult`
- Aim for high code coverage but focus on meaningful tests
- Use mocking/stubbing only when necessary for isolation
- Test both happy path and error scenarios

Example test:
```csharp
[Fact]
public async Task LoadPluginAsync_WithValidPath_ReturnsLoadedPlugin()
{
    // Arrange
    var pluginPath = "path/to/plugin.dll";
    
    // Act
    var plugin = await _loader.LoadPluginAsync(pluginPath);
    
    // Assert
    Assert.NotNull(plugin);
    Assert.Equal("expected-name", plugin.Name);
}
```

## Committing Changes

### Commit Message Format

Follow conventional commits format for consistency:

```
<type>: <subject>

<body>

<footer>
```

**Types:**
- `feat`: A new feature
- `fix`: A bug fix
- `docs`: Documentation only changes
- `refactor`: Code change that neither fixes a bug nor adds a feature
- `test`: Adding missing tests or correcting existing tests
- `chore`: Changes to build process, dependencies, or tools

**Subject line:**
- Use imperative mood ("add feature" not "added feature")
- Don't capitalize first letter
- No period at the end
- Limit to 50 characters

**Body:**
- Explain *what* and *why*, not *how*
- Wrap at 72 characters
- Separate from subject with a blank line

**Example:**
```
feat: add webhook support for plugin lifecycle events

Implement webhook publishing for plugin loaded, unloaded, and failed events.
Add WebhookConfiguration and WebhookHandler classes for managing webhooks.
Webhooks allow external systems to be notified of plugin lifecycle changes.

Closes #123
Relates to #456
```

### Before Committing

```bash
# Format code
dotnet format

# Verify no compiler warnings
dotnet build

# Run tests
dotnet test

# Check git status
git status

# Stage changes
git add <files>

# Commit with message
git commit -m "feat: your feature description"

# Push to your fork
git push origin your-branch-name
```

## Pull Request Process

### Preparation

1. **Update your branch** with upstream changes:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```

2. **Push to your fork**:
   ```bash
   git push origin your-branch-name
   ```

3. **Create a pull request** on GitHub with a descriptive title and detailed description

### PR Description Template

```markdown
## Description
Brief description of what this PR does.

## Type of Change
- [ ] New feature
- [ ] Bug fix
- [ ] Documentation update
- [ ] Refactoring
- [ ] Other: describe

## Testing
Describe how you tested this change:
- [ ] Unit tests added/updated
- [ ] Manual testing performed
- [ ] Tested scenario: [describe]

## Checklist
- [ ] Code follows project style guidelines
- [ ] `dotnet format` has been run
- [ ] New tests added for new functionality
- [ ] All tests pass: `dotnet test`
- [ ] No compiler warnings
- [ ] Documentation updated (if needed)
- [ ] Commit messages follow conventions
- [ ] Branch rebased on latest main

## Related Issues
Closes #[issue-number]
Related to #[issue-number]
```

### Review Process

- All pull requests require at least one review before merging
- Address feedback by making additional commits (don't force push unless requested)
- Keep discussions professional and constructive
- PR author or maintainer can mark conversations as resolved

### After Approval

- Maintainers will merge approved PRs
- Your branch will be automatically deleted after merge
- Celebrate your contribution! 🎉

## Reporting Issues

### Before Submitting an Issue

1. **Search existing issues** - your issue may already be reported
2. **Check the documentation** - your question may be answered there
3. **Review the FAQ** - [docs/faq.md](docs/faq.md)

### Issue Report Template

**Title**: Clear, descriptive title

**Description**:
```markdown
## Environment
- .NET SDK version: [e.g., 10.0.0]
- OS: [Windows/Linux/macOS]
- Plugin Engine version: [e.g., 1.0.0]

## Describe the Bug
Clear and concise description of the issue.

## Steps to Reproduce
1. Step 1
2. Step 2
3. Step 3

## Expected Behavior
What should happen?

## Actual Behavior
What actually happens?

## Code Example
```csharp
// Minimal code that reproduces the issue
```

## Error Message/Stack Trace
Full error message and stack trace if applicable.

## Additional Context
Any other relevant information.
```

### Types of Issues

- **Bug Report**: Report unexpected behavior
- **Feature Request**: Suggest an enhancement
- **Documentation**: Unclear or missing documentation
- **Question**: Ask for help or clarification

## License

By contributing to dotnet-plugin-engine, you agree that your contributions will be licensed under its [MIT License](LICENSE).

The project is licensed under the MIT License - Copyright (c) 2026 Vladyslav Zaiets

## Questions?

- **Documentation**: Check [docs/](docs/) directory
- **FAQ**: See [docs/faq.md](docs/faq.md)
- **GitHub Issues**: Open an issue for questions
- **Author**: [https://sarmkadan.com](https://sarmkadan.com)

Thank you for contributing to dotnet-plugin-engine! Your efforts help make this project better for everyone.
