using PS2Home.Core;

namespace PS2Home.Infrastructure;

/// <summary>
/// Scans the filesystem recursively for PS2 disc images (.iso).
/// Handles permissions errors gracefully and guards against symlink loops.
/// </summary>
public class FileSystemGameScanner : IGameScanner
{
    /// <summary>
    /// Default supported disc image extensions.
    /// Note: Formats such as .chd, .cso, .bin/.cue are planned for future phases once verified against PCSX2 CLI.
    /// </summary>
    public static readonly string[] DefaultSupportedExtensions = [".iso"];

    private readonly HashSet<string> _supportedExtensions;

    public IReadOnlyList<string> SupportedExtensions => [.. _supportedExtensions];

    public FileSystemGameScanner(IEnumerable<string>? customExtensions = null)
    {
        var extensions = customExtensions ?? DefaultSupportedExtensions;
        _supportedExtensions = new HashSet<string>(extensions, StringComparer.OrdinalIgnoreCase);
    }

    public async Task<IReadOnlyList<GameEntry>> ScanFoldersAsync(
        IEnumerable<string> folderPaths,
        IProgress<ScanProgress>? progress = null,
        CancellationToken cancellationToken = default)
    {
        var foundGames = new Dictionary<string, GameEntry>(StringComparer.OrdinalIgnoreCase);
        var visitedDirectories = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        int foldersScanned = 0;

        foreach (var folder in folderPaths)
        {
            if (string.IsNullOrWhiteSpace(folder))
                continue;

            cancellationToken.ThrowIfCancellationRequested();

            var normalizedFolder = Path.GetFullPath(folder).TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
            if (!Directory.Exists(normalizedFolder))
                continue;

            await ScanDirectoryRecursiveAsync(
                normalizedFolder,
                foundGames,
                visitedDirectories,
                progress,
                () => ++foldersScanned,
                cancellationToken);
        }

        return [.. foundGames.Values];
    }

    private async Task ScanDirectoryRecursiveAsync(
        string directoryPath,
        Dictionary<string, GameEntry> foundGames,
        HashSet<string> visitedDirectories,
        IProgress<ScanProgress>? progress,
        Func<int> incrementFolders,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Symlink loop prevention: resolve canonical path if it's a link
        string canonicalPath = directoryPath;
        try
        {
            var dirInfo = new DirectoryInfo(directoryPath);
            if (dirInfo.LinkTarget != null)
            {
                var resolved = dirInfo.ResolveLinkTarget(returnFinalTarget: true);
                if (resolved != null)
                {
                    canonicalPath = resolved.FullName;
                }
            }
        }
        catch
        {
            // If link resolution fails, proceed with the original path
        }

        if (!visitedDirectories.Add(canonicalPath))
        {
            // Already visited this physical directory; stop to avoid loops
            return;
        }

        int folderCount = incrementFolders();
        progress?.Report(new ScanProgress(folderCount, foundGames.Count, directoryPath));

        // Scan files in current directory
        try
        {
            var dirInfo = new DirectoryInfo(directoryPath);
            foreach (var file in dirInfo.EnumerateFiles())
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (_supportedExtensions.Contains(file.Extension))
                {
                    var id = GameEntry.CreateId(file.FullName);
                    if (!foundGames.ContainsKey(id))
                    {
                        var entry = new GameEntry
                        {
                            Id = id,
                            Title = TitleFormatter.Format(file.Name),
                            FullPath = file.FullName,
                            FileSizeBytes = file.Length,
                            AddedAt = DateTimeOffset.UtcNow,
                            LastScannedAt = DateTimeOffset.UtcNow,
                            IsMissing = false
                        };
                        foundGames[id] = entry;
                        progress?.Report(new ScanProgress(folderCount, foundGames.Count, file.FullName));
                    }
                }
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Inaccessible folder; skip without crashing
            return;
        }
        catch (DirectoryNotFoundException)
        {
            return;
        }
        catch (IOException)
        {
            return;
        }

        // Recurse into subdirectories
        try
        {
            var dirInfo = new DirectoryInfo(directoryPath);
            foreach (var subDir in dirInfo.EnumerateDirectories())
            {
                cancellationToken.ThrowIfCancellationRequested();

                // Skip hidden or system folders (e.g. .git, Recycled)
                if (subDir.Attributes.HasFlag(FileAttributes.Hidden) || subDir.Name.StartsWith('.'))
                    continue;

                await ScanDirectoryRecursiveAsync(
                    subDir.FullName,
                    foundGames,
                    visitedDirectories,
                    progress,
                    incrementFolders,
                    cancellationToken);
            }
        }
        catch (UnauthorizedAccessException)
        {
            // Inaccessible subdirectories; skip
        }
        catch (DirectoryNotFoundException)
        {
        }
        catch (IOException)
        {
        }

        await Task.Yield();
    }
}
