using System.IO;

namespace TaskDesk.Services;

public enum ResourceLibrarySource
{
    Config,
    SiblingFolder,
    UserSelectionRequired
}

public sealed record ResourceLibraryLocation(string? FullPath, string? RelativePath, ResourceLibrarySource Source)
{
    public bool IsAvailable => FullPath is not null && RelativePath is not null;
}

public sealed class ResourceLibraryLocator
{
    private readonly TaskDeskPaths _paths;
    private readonly ConfigStore _configStore;

    public ResourceLibraryLocator(TaskDeskPaths paths, ConfigStore configStore)
    {
        _paths = paths;
        _configStore = configStore;
    }

    public ResourceLibraryLocation Locate()
    {
        var configuredPath = _configStore.Load().ResourceLibraryPath;
        if (TryResolveRelative(configuredPath, out var configuredFullPath, out var configuredRelativePath))
        {
            return new ResourceLibraryLocation(configuredFullPath, configuredRelativePath, ResourceLibrarySource.Config);
        }

        var siblingPath = Path.Combine(_paths.SoftwareRoot, "资源库");
        if (Directory.Exists(siblingPath))
        {
            return new ResourceLibraryLocation(siblingPath, "资源库", ResourceLibrarySource.SiblingFolder);
        }

        return new ResourceLibraryLocation(null, null, ResourceLibrarySource.UserSelectionRequired);
    }

    public ResourceLibraryLocation SaveSelection(string selectedDirectory)
    {
        var fullPath = Path.GetFullPath(selectedDirectory);
        if (!Directory.Exists(fullPath))
        {
            throw new DirectoryNotFoundException(fullPath);
        }

        var relativePath = Path.GetRelativePath(_paths.SoftwareRoot, fullPath);
        if (Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException("资源库目录必须能转换为相对软件根目录的路径。", nameof(selectedDirectory));
        }

        _configStore.SaveResourceLibraryPath(relativePath);
        return new ResourceLibraryLocation(fullPath, relativePath, ResourceLibrarySource.Config);
    }

    private bool TryResolveRelative(string? relativePath, out string? fullPath, out string? normalizedRelativePath)
    {
        fullPath = null;
        normalizedRelativePath = null;
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
        {
            return false;
        }

        var candidate = Path.GetFullPath(Path.Combine(_paths.SoftwareRoot, relativePath));
        if (!Directory.Exists(candidate))
        {
            return false;
        }

        fullPath = candidate;
        normalizedRelativePath = Path.GetRelativePath(_paths.SoftwareRoot, candidate);
        return true;
    }
}
