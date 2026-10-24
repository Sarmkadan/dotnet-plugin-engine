# Contributing to dotnet-plugin-engine

Thanks for your interest in contributing! This is a .NET class library
(`PluginEngine`, NuGet `Zaiets.dotnet.plugin.engine`).

## Prerequisites

- **.NET 10 SDK** (the projects target `net10.0`; CI uses `10.0.x`)
- Git

Check your setup with `dotnet --list-sdks`.

## Build

The repository root contains two solution files, so name the solution explicitly:

```bash
dotnet restore dotnet-plugin-engine.sln
dotnet build dotnet-plugin-engine.sln -c Release
```

Or use `make build`, which wraps the same commands.

## Run tests

```bash
dotnet test dotnet-plugin-engine.sln -c Release
```

Tests use xUnit, FluentAssertions and Moq and live in `tests/dotnet-plugin-engine.Tests/`.
Some test files are excluded from the test project on purpose; do not re-include
them without fixing them first.

## Coding conventions

- Follow `.editorconfig` (4-space indent, LF line endings, braces required).
  Run `dotnet format` before committing; `make format-check` must pass.
- Nullable reference types are enabled. Every source file starts with `#nullable enable`.
- Warnings are errors in the lint build: `make lint` builds with `TreatWarningsAsErrors=true`.
- One public type per file, named after the type.
- XML documentation is required on all public API (`GenerateDocumentationFile` is on).
- Use `async`/`await` with `*Async` names and `CancellationToken` parameters.
- Keep the existing author header block in each file.

## Branching and pull requests

1. Branch from the latest `main` (`feat/...`, `fix/...`, `docs/...`).
2. Keep PRs small and focused on one change.
3. Use conventional commit messages, for example `feat(Scope): ...` or `fix: ...`.
4. Add or update tests for the change.
5. Open a PR against `main`. In the description, explain what changed and why,
   and link the issue it addresses.
6. CI must be green before review. It runs restore, build, test and format checks.

## Reporting bugs

Search existing issues first. Then open a new issue on GitHub with:

- the library version and the .NET SDK version (`dotnet --list-sdks`)
- the operating system
- minimal steps to reproduce, the expected result and the actual result
- the exception or log output, if any

Security problems should not be reported in public issues; see [SECURITY.md](SECURITY.md).

## License

By contributing, you agree that your work is licensed under the MIT License.
