# VersionHelperExtensions

A set of extension methods that provide semantic‑version comparison, validation, and parsing capabilities for string‑based version identifiers. The methods operate on the instance string and return results without modifying the original value.

## API

### IsGreaterThan
```csharp
public static bool IsGreaterThan(this string version, string other)
```
Determines whether the `version` operand is greater than the `other` operand according to semantic versioning rules.  
- **Parameters**  
  - `version`: The version string to evaluate (the instance on which the method is called).  
  - `other`: The version string to compare against.  
- **Return value**  
  - `true` if `version` is greater than `other`; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if either `version` or `other` is `null`.  
  - `FormatException` if either string cannot be parsed as a valid semantic version.

### IsLessThan
```csharp
public static bool IsLessThan(this string version, string other)
```
Determines whether the `version` operand is less than the `other` operand according to semantic versioning rules.  
- **Parameters**  
  - `version`: The version string to evaluate.  
  - `other`: The version string to compare against.  
- **Return value**  
  - `true` if `version` is less than `other`; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if either `version` or `other` is `null`.  
  - `FormatException` if either string cannot be parsed as a valid semantic version.

### IsEqualTo
```csharp
public static bool IsEqualTo(this string version, string other)
```
Determines whether the `version` operand is equal to the `other` operand according to semantic versioning rules (ignoring build metadata).  
- **Parameters**  
  - `version`: The version string to evaluate.  
  - `other`: The version string to compare against.  
- **Return value**  
  - `true` if `version` is equal to `other`; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if either `version` or `other` is `null`.  
  - `FormatException` if either string cannot be parsed as a valid semantic version.

### GetVersionInfo
```csharp
public static ParsedVersionInfo GetVersionInfo(this string version)
```
Parses a semantic version string into its constituent parts.  
- **Parameters**  
  - `version`: The version string to parse.  
- **Return value**  
  - A `ParsedVersionInfo` instance containing the major, minor, patch, pre‑release identifiers, and build metadata.  
- **Exceptions**  
  - `ArgumentNullException` if `version` is `null`.  
  - `FormatException` if `version` does not conform to the semantic versioning specification.

### IsValidSemanticVersion
```csharp
public static bool IsValidSemanticVersion(this string version)
```
Checks whether a string represents a valid semantic version.  
- **Parameters**  
  - `version`: The version string to validate.  
- **Return value**  
  - `true` if `version` is a valid semantic version; otherwise `false`.  
- **Exceptions**  
  - `ArgumentNullException` if `version` is `null`.  
  - No exception is thrown for malformed version strings; the method simply returns `false`.

## Usage

```csharp
string current = "2.1.0-beta.3";
string latest  = "2.1.0";

if (current.IsValidSemanticVersion() && latest.IsValidSemanticVersion())
{
    if (current.IsLessThan(latest))
    {
        Console.WriteLine("Update available.");
    }
    else if (current.IsEqualTo(latest))
    {
        Console.WriteLine("Up to date.");
    }
    else
    {
        Console.WriteLine("Current version is ahead of the published release.");
    }
}
```

```csharp
string tag = "v1.2.3+build.456";
ParsedVersionInfo info = tag.GetVersionInfo();

Console.WriteLine($"Major: {info.Major}");
Console.WriteLine($"Minor: {info.Minor}");
Console.WriteLine($"Patch: {info.Patch}");
Console.WriteLine($"Pre-release: {string.Join(".", info.PreRelease)}");
Console.WriteLine($"Build: {string.Join(".", info.Build)}");
```

## Notes
- All methods treat the input strings as case‑sensitive and do not perform any trimming; leading or trailing whitespace will cause a `FormatException` (or `false` for `IsValidSemanticVersion`).  
- Pre‑release identifiers are compared lexicographically with numeric identifiers having lower precedence than non‑numeric ones, as defined by the SemVer spec. Build metadata is ignored for equality and ordering comparisons.  
- The extension methods are pure; they rely only on their arguments and therefore are thread‑safe. No static state is mutated, so concurrent invocation from multiple threads is safe.  
- Passing `null` for any version argument results in an `ArgumentNullException`; the methods do not treat `null` as an empty or default version.  
- `GetVersionInfo` throws on the first deviation from the SemVer grammar; if lenient parsing is required, validate with `IsValidSemanticVersion` beforehand.
