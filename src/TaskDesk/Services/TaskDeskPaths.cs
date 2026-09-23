using System.IO;

namespace TaskDesk.Services;

public sealed class TaskDeskPaths
{
    public TaskDeskPaths(string? softwareRoot = null)
    {
        SoftwareRoot = Path.GetFullPath(softwareRoot ?? AppContext.BaseDirectory);
    }

    public string SoftwareRoot { get; }

    public string ConfigFile => Path.Combine(SoftwareRoot, "config.json");

    public string DataDirectory => Path.Combine(SoftwareRoot, "data");

    public string TasksFile => Path.Combine(DataDirectory, "tasks.json");

    public string PublishDirectory => Path.Combine(SoftwareRoot, "publish");
}
