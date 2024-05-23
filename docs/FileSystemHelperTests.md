# FileSystemHelperTests

Unit test class for `FileSystemHelper` providing test coverage for filesystem operations including directory management, file discovery, metadata retrieval, and recursive deletion.

## API

### `FileSystemHelperTests`
Test fixture containing test methods for filesystem helper operations. Initializes a temporary test directory and cleans it up after each test.

### `EnsureDirectoryExists_WithNonExistentDirectory_CreatesDirectory`
Verifies that calling `FileSystemHelper.EnsureDirectoryExists` on a non-existent path creates the directory and returns `true`.

### `EnsureDirectoryExists_WithExistingDirectory_ReturnsTrue`
Verifies that calling `FileSystemHelper.EnsureDirectoryExists` on an existing directory returns `true` without modifying the filesystem.

### `EnsureDirectoryExists_WithInvalidPath_ReturnsFalse`
Verifies that calling `FileSystemHelper.EnsureDirectoryExists` with an invalid path (e.g., containing invalid characters) returns `false` and does not throw.

### `DiscoverPlugins_WithNonExistentDirectory_ReturnsEmptyList`
Verifies that calling `FileSystemHelper.DiscoverPlugins` on a non-existent directory returns an empty list without throwing.

### `DiscoverPlugins_WithEmptyDirectory_ReturnsEmptyList`
Verifies that calling `FileSystemHelper.DiscoverPlugins` on an empty directory returns an empty list.

### `DiscoverPlugins_WithDllFiles_ReturnsAllDllFiles`
Verifies that calling `FileSystemHelper.DiscoverPlugins` on a directory containing `.dll` files returns a list containing all `.dll` files.

### `DiscoverPlugins_ExcludesFilesStartingWithUnderscore`
Verifies that calling `FileSystemHelper.DiscoverPlugins` excludes files whose names start with an underscore (`_`).

### `GetFileInfo_WithExistingFile_ReturnsFileSizeAndModifiedTime`
Verifies that calling `FileSystemHelper.GetFileInfo` on an existing file returns a non-null object containing the file size and last modified time.

### `GetFileInfo_WithNonExistentFile_ReturnsNull`
Verifies that calling `FileSystemHelper.GetFileInfo` on a non-existent file returns `null`.

### `GetFileInfo_WithLargeFile_ReturnsCorrectSize`
Verifies that calling `FileSystemHelper.GetFileInfo` on a large file returns the correct size without overflow or truncation.

### `DeleteDirectoryRecursive_WithExistingDirectory_DeletesDirectory`
Verifies that calling `FileSystemHelper.DeleteDirectoryRecursive` on an existing directory deletes the directory and all its contents.

### `DeleteDirectoryRecursive_WithNonExistentDirectory_ReturnsTrue`
Verifies that calling `FileSystemHelper.DeleteDirectoryRecursive` on a non-existent directory returns `true` without throwing.

### `SafeCopyFile_WithValidSourceAndDestination_CopiesFile`
Verifies that calling `FileSystemHelper.SafeCopyFile` with valid source and destination paths copies the file and returns `true`.

### `SafeCopyFile_WithNonExistentSource_ReturnsFalse`
Verifies that calling `FileSystemHelper.SafeCopyFile` with a non-existent source returns `false` without throwing.

### `SafeCopyFile_WhenDestinationExistsAndOverwriteDisabled_ReturnsFalse`
Verifies that calling `FileSystemHelper.SafeCopyFile` with an existing destination and overwrite disabled returns `false`.

### `SafeCopyFile_WhenDestinationExistsAndOverwriteEnabled_CopiesFile`
Verifies that calling `FileSystemHelper.SafeCopyFile` with an existing destination and overwrite enabled copies the file and returns `true`.

### `GetDirectorySize_WithEmptyDirectory_ReturnsZero`
Verifies that calling `FileSystemHelper.GetDirectorySize` on an empty directory returns `0`.

### `GetDirectorySize_WithFiles_ReturnsTotalSize`
Verifies that calling `FileSystemHelper.GetDirectorySize` on a directory containing files returns the total size of all files.

### `GetDirectorySize_WithNonExistentDirectory_ReturnsZero`
Verifies that calling `FileSystemHelper.GetDirectorySize` on a non-existent directory returns `0`.

## Usage
