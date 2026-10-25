# SemanticVersion

`SemanticVersion` is a mutable class that holds the parts of a semantic version: numeric `Major`, `Minor` and `Patch`, plus an optional prerelease label and build metadata. It is produced by `IVersioningService.ParseVersion` and rendered back to text with `ToString`.

`VersionPart` is the enum that selects which numeric component `IncrementVersion` bumps.

Both types are declared in `src/PluginEngine/Services/Abstractions/IVersioningService.cs` and live in the `PluginEngine.Services.Abstractions` namespace.

## API

### `public sealed class SemanticVersion`

A semantic version made of a numeric core and optional prerelease and metadata labels.

- **Properties**
  - `int Major { get; set; }`: The major version number. Defaults to `0`.
  - `int Minor { get; set; }`: The minor version number. Defaults to `0`.
  - `int Patch { get; set; }`: The patch version number. Defaults to `0`.
  - `string Prerelease { get; set; }`: The prerelease label (for example `beta.1`). Defaults to `string.Empty`.
  - `string Metadata { get; set; }`: The build metadata (for example `build.42`). Defaults to `string.Empty`.

All properties are settable, so instances can be built by hand as well as by `ParseVersion`.

---

### `public override string ToString()`

Formats the version as `Major.Minor.Patch[-Prerelease][+Metadata]`.

- The prerelease segment is emitted only when `Prerelease` is not null, empty or whitespace.
- The metadata segment is emitted only when `Metadata` is not null, empty or whitespace.
- **Returns**
  - For example, `1.2.3`, `1.2.3-beta.1`, or `1.2.3-beta.1+build.42`.
  - Values are written as stored; no validation or normalization is applied.

---

### `public enum VersionPart`

Identifies the numeric component to increment.

| Member  | Value | Effect in `IncrementVersion`                              |
|---------|-------|-----------------------------------------------------------|
| `Major` | `0`   | Major + 1, Minor and Patch reset to `0`.                  |
| `Minor` | `1`   | Minor + 1, Patch reset to `0`, Major unchanged.           |
| `Patch` | `2`   | Patch + 1, Major and Minor unchanged.                     |

Any other integer value passed as `VersionPart` causes `IncrementVersion` to throw `ArgumentException`.

---

## How the types are used

### `IVersioningService.ParseVersion(string versionString)`

`VersioningService.ParseVersion` (`src/PluginEngine/Services/Implementations/VersioningService.cs`) builds a `SemanticVersion` from a string:

1. The string must not be null, empty or whitespace; otherwise an `ArgumentException` is thrown.
2. The core is everything before the first `-` or `+`. It is parsed with `System.Version.TryParse`:
   - `Major` = `Version.Major`
   - `Minor` = `Version.Minor`
   - `Patch` = `Version.Build`
3. If the string contains `-`, the text after the first `-` is the prerelease. If that text also contains `+`, the part before the `+` is `Prerelease` and the part after it is `Metadata`.
4. Otherwise, if the string contains `+` without a `-`, the text after the first `+` is `Metadata`.

Example:

```csharp
var version = versionService.ParseVersion("2.4.1-rc.2+sha.5114f85");
// Major = 2, Minor = 4, Patch = 1, Prerelease = "rc.2", Metadata = "sha.5114f85"
```

### `IVersioningService.IncrementVersion(string currentVersion, VersionPart part)`

`VersioningService.IncrementVersion` works on the string form and does not use `SemanticVersion`. It parses `currentVersion` with `System.Version.TryParse` and returns a new `Version` string:

| Input       | `VersionPart` | Result    |
|-------------|---------------|-----------|
| `1.2.3`     | `Major`       | `2.0.0`   |
| `1.2.3`     | `Minor`       | `1.3.0`   |
| `1.2.3`     | `Patch`       | `1.2.4`   |

Behavior to be aware of:

- If `currentVersion` cannot be parsed by `System.Version`, an `ArgumentException` is thrown (parameter `currentVersion`).
- Prerelease and metadata are **not** carried over, and they are not accepted as input: `1.2.3-beta` fails to parse and throws. To increment a version that has a prerelease label, the label must be handled outside this method.
- Four-part versions (`1.2.3.4`) are accepted but the revision component is dropped in the result (`2.0.0` for `Major`).

### `IVersioningService.ValidateVersion(string version)`

`ValidateVersion` checks the string with `System.Version.TryParse`. Strings containing a prerelease or metadata suffix (for example `1.2.3-beta`) are therefore reported as invalid, even though `ParseVersion` accepts them.

---

## Known limitations

- `ParseVersion` does not validate the core. If `System.Version.TryParse` fails on the part before the first `-` or `+`, `Major`, `Minor` and `Patch` silently stay at `0`.
- A core with fewer than three numeric parts gives `Patch = -1`, because `System.Version` reports a missing `Build` as `-1`. For example, `ParseVersion("1.2")` yields `Patch = -1`.
- `ParseVersion` splits at the first `-` or `+` found in the string, so metadata written before a `-` is not recognized. For example, `1.0.0+build-7` is read as `Prerelease = "7"` with no metadata.

## Usage

```csharp
using PluginEngine.Services.Abstractions;
using PluginEngine.Services.Implementations;

IVersioningService versioning = new VersioningService();

SemanticVersion parsed = versioning.ParseVersion("1.4.0-beta.3+exp.sha.1");
Console.WriteLine(parsed.ToString());   // 1.4.0-beta.3+exp.sha.1

string next = versioning.IncrementVersion("1.4.0", VersionPart.Minor);
Console.WriteLine(next);                // 1.5.0
```
