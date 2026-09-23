using System.IO;

using System.Text.Json;
using System.Text.Json.Serialization;
using TaskDesk.Models;

namespace TaskDesk.Services;

public sealed class TaskStore
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly TaskDeskPaths _paths;

    public TaskStore(TaskDeskPaths paths)
    {
        _paths = paths;
        IsWritable = CheckWritable();
    }

    public bool IsWritable { get; }

    public IReadOnlyList<InstallTask> Load()
    {
        if (!File.Exists(_paths.TasksFile))
        {
            return [];
        }

        var json = File.ReadAllText(_paths.TasksFile);
        return JsonSerializer.Deserialize<List<InstallTask>>(json, JsonOptions) ?? [];
    }

    public void Save(IEnumerable<InstallTask> tasks)
    {
        if (!IsWritable)
        {
            throw new InvalidOperationException("软件根目录不可写，当前处于只读运行模式。");
        }

        Directory.CreateDirectory(_paths.DataDirectory);
        var json = JsonSerializer.Serialize(tasks, JsonOptions);
        var temporaryFile = _paths.TasksFile + ".tmp";
        File.WriteAllText(temporaryFile, json);
        File.Move(temporaryFile, _paths.TasksFile, true);
    }

    private bool CheckWritable()
    {
        try
        {
            Directory.CreateDirectory(_paths.DataDirectory);
            var probe = Path.Combine(_paths.DataDirectory, $".taskdesk-write-test-{Guid.NewGuid():N}");
            using (File.Open(probe, FileMode.CreateNew, FileAccess.Write, FileShare.None))
            {
            }

            File.Delete(probe);
            return true;
        }
        catch (IOException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }
}
