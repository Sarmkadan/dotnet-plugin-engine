# FileSystemHelper

The `FileSystemHelper` class provides a static utility layer for performing common file system operations required by the plugin engine, including plugin discovery, directory management, and safe file manipulation. It abstracts away low-level `System.IO` calls to enforce consistent error handling, atomic operations where possible, and standardized return types for status checks, ensuring reliable interaction with the host environment's storage without exposing raw exceptions to the core engine logic.

## API

### `FileSystemHelper`
The static class constructor. As a static utility class, it cannot be instantiated; this member signifies the type definition containing the following static methods.

### `EnsureDirectoryExists`
Creates the specified directory path if it does not already exist.
*   **Parameters**: `string path` – The absolute or relative path of the directory to ensure.
*   **Returns**: `bool` – `true` if the directory was created successfully or already existed; `false` if creation failed due to permissions or invalid characters.
*   **Throws**: Does not throw standard IO exceptions; failures are captured and returned as `false`.

### `DiscoverPlugins`
Scans a target directory for valid plugin assemblies based on engine conventions.
*   **Parameters**: `string searchPath` – The root directory to scan recursively.
*   **Returns**: `IEnumerable<string>` – A collection of absolute file paths pointing to discovered plugin files. Returns an empty enumerable if no plugins are found or the path is invalid.
*   **Throws**: Does not throw; inaccessible directories are skipped silently.

### `GetFileInfo`
Retrieves metadata for a specific file without throwing exceptions on missing files.
*   **Parameters**: `string filePath` – The path to the file to inspect.
*   **Returns**: `(long Size, DateTime Modified)?` – A nullable tuple containing the file size in bytes and the last modified timestamp. Returns `null` if the file does not exist or cannot be accessed.
*   **Throws**: Does not throw; access errors result in a `null` return.

### `SafeCopyFile`
Attempts to copy a file from a source to a destination with overwrite protection.
*   **Parameters**: `string sourcePath`, `string destinationPath` – The paths for the source and target files.
*   **Returns**: `bool` – `true` if the copy operation completed successfully; `false` if the source was missing, the destination was locked, or permissions were insufficient.
*   **Throws**: Does not throw; all IO errors are handled internally returning `false`.

### `DeleteDirectoryRecursive`
Removes a directory and all its contents.
*   **Parameters**: `string path` – The path of the directory to delete.
*   **Returns**: `bool` – `true` if the directory and all contents were deleted; `false` if the directory did not exist or if any file within the tree was locked or protected.
*   **Throws**: Does not throw; partial failures result in `false`.

### `GetDirectorySize`
Calculates the total size of all files within a directory tree.
*   **Parameters**: `string path` – The root directory to calculate size for.
*   **Returns**: `long` – The total size in bytes. Returns `0` if the directory does not exist or cannot be read.
*   **Throws**: Does not throw; inaccessible subdirectories are skipped.

### `CreateBackup`
Generates a timestamped backup copy of a specific file or directory.
*   **Parameters**: `string sourcePath` – The path to the item to back up.
*   **Returns**: `string?` – The full path to the newly created backup item if successful; `null` if the backup could not be created.
*   **Throws**: Does not throw; failures return `null`.

### `IsDirectoryWritable`
Verifies if the current process has write permissions for a specific directory.
*   **Parameters**: `string path` – The directory path to test.
*   **Returns**: `bool` – `true` if a test file can be created and deleted successfully; `false` otherwise.
*   **Throws**: Does not throw; permission checks are performed via trial operation.

## Usage

### Plugin Discovery and Validation
The following example demonstrates scanning a plugins folder, verifying the existence of each discovered file, and retrieving their metadata before loading.

```csharp
using System;
using System.Linq;

public class PluginLoader
{
    public void LoadPlugins(string pluginsRoot)
    {
        var pluginPaths = FileSystemHelper.DiscoverPlugins(pluginsRoot);

        foreach (var path in pluginPaths)
        {
            var info = FileSystemHelper.GetFileInfo(path);
            
            if (info.HasValue)
            {
                Console.WriteLine($"Found plugin: {path}");
                Console.WriteLine($"Size: {info.Value.Size} bytes, Modified: {info.Value.Modified}");
                
                // Proceed with loading logic...
            }
            else
            {
                Console.WriteLine($"Plugin file inaccessible or missing: {path}");
            }
        }
    }
}
```

### Safe Deployment with Backup
This example illustrates a deployment scenario where an existing configuration is backed up before a new file is copied, ensuring the directory structure exists and the operation is verified.

```csharp
using System;

public class ConfigDeployer
{
    public bool DeployConfig(string sourceConfig, string targetDir, string fileName)
    {
        if (!FileSystemHelper.EnsureDirectoryExists(targetDir))
        {
            return false;
        }

        var targetPath = System.IO.Path.Combine(targetDir, fileName);
        
        // If target exists, create a backup first
        if (FileSystemHelper.GetFileInfo(targetPath).HasValue)
        {
            var backupPath = FileSystemHelper.CreateBackup(targetPath);
            if (backupPath == null)
            {
                return false; // Failed to create backup, abort deployment
            }
            Console.WriteLine($"Backup created at: {backupPath}");
        }

        // Attempt the copy
        if (!FileSystemHelper.SafeCopyFile(sourceConfig, targetPath))
        {
            Console.WriteLine("Failed to copy new configuration.");
            return false;
        }

        return true;
    }
}
```

## Notes

*   **Exception Handling Strategy**: All members in `FileSystemHelper` follow a "try-return" pattern rather than throwing exceptions for common IO failures (e.g., missing files, access denied, locked files). Callers must check boolean return values or nullable results to determine success.
*   **Atomicity**: Operations like `SafeCopyFile` and `CreateBackup` attempt to be atomic but depend on the underlying OS file system capabilities. If `SafeCopyFile` returns `false`, the destination file state is guaranteed to be unchanged, but partial temporary files may exist depending on the failure point.
*   **Thread Safety**: As a static class relying on stateless `System.IO` calls, `FileSystemHelper` is thread-safe for concurrent read operations. However, concurrent write operations targeting the same file or directory paths (e.g., two threads calling `DeleteDirectoryRecursive` on the same path) are not coordinated by this class and may result in race conditions or `false` return values.
*   **Path Resolution**: Relative paths passed to these methods are resolved against the current working directory of the host process. It is recommended to use absolute paths to ensure predictable behavior in plugin engine contexts.
*   **Write Verification**: `IsDirectoryWritable` performs a physical test by creating and immediately deleting a temporary file. This ensures accurate permission checking but may incur slight I/O overhead compared to metadata-only checks.
