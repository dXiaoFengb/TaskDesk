using System.IO;

using System.Text.Json;

namespace TaskDesk.Services;

public sealed class ConfigStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true
    };

    private readonly TaskDeskPaths _paths;

    public ConfigStore(TaskDeskPaths paths)
    {
        _paths = paths;
    }

    public ConfigDocument Load()
    {
        if (!File.Exists(_paths.ConfigFile))
        {
            return new ConfigDocument();
        }

        var json = File.ReadAllText(_paths.ConfigFile);
        return JsonSerializer.Deserialize<ConfigDocument>(json, JsonOptions) ?? new ConfigDocument();
    }

    public void SaveResourceLibraryPath(string relativePath)
    {
        if (string.IsNullOrWhiteSpace(relativePath) || Path.IsPathRooted(relativePath))
        {
            throw new ArgumentException("资源库配置路径必须是非空相对路径。", nameof(relativePath));
        }

        Directory.CreateDirectory(_paths.SoftwareRoot);
        var document = Load();
        document.ResourceLibraryPath = relativePath;
        var json = JsonSerializer.Serialize(document, JsonOptions);
        var temporaryFile = _paths.ConfigFile + ".tmp";
        File.WriteAllText(temporaryFile, json);
        File.Move(temporaryFile, _paths.ConfigFile, true);
    }
}
