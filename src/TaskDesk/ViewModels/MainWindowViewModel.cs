using System.IO;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using TaskDesk.Models;
using TaskDesk.Services;

namespace TaskDesk.ViewModels;

public sealed class MainWindowViewModel : INotifyPropertyChanged
{
    private readonly TaskDeskPaths _paths;
    private readonly TaskStore _taskStore;
    private readonly ConfigStore _configStore;
    private readonly ResourceLibraryLocator _resourceLocator;
    private readonly ResourceLibraryBrowser _resourceBrowser = new();
    private readonly ResourceExecutionService _resourceExecutionService = new();
    private InstallTask? _selectedTask;
    private ResourceEntry? _selectedResource;
    private string? _resourceRoot;
    private string? _currentResourceDirectory;
    private string _previewText = "选择要预览的文件。";
    private string _statusMessage = "正在加载 TaskDesk。";
    private InstallTask? _clipboardTask;
    private bool _clipboardIsCut;

    public MainWindowViewModel()
    {
        _paths = new TaskDeskPaths();
        _taskStore = new TaskStore(_paths);
        _configStore = new ConfigStore(_paths);
        _resourceLocator = new ResourceLibraryLocator(_paths, _configStore);
        Tasks = new ObservableCollection<InstallTask>(_taskStore.Load());
        ResourceEntries = new ObservableCollection<ResourceEntry>();
        NewTaskCommand = new RelayCommand(_ => AddTask());
        DeleteTaskCommand = new RelayCommand(_ => DeleteSelectedTask(), _ => SelectedTask is not null && !IsReadOnly);
        CopyTaskCommand = new RelayCommand(_ => CopySelectedTask(), _ => SelectedTask is not null);
        CutTaskCommand = new RelayCommand(_ => CutSelectedTask(), _ => SelectedTask is not null && !IsReadOnly);
        PasteTaskCommand = new RelayCommand(_ => PasteTask(), _ => _clipboardTask is not null && !IsReadOnly);
        MarkInProgressCommand = new RelayCommand(_ => SetSelectedStatus(InstallTaskStatus.InProgress), _ => SelectedTask is not null && !IsReadOnly);
        MarkCompletedCommand = new RelayCommand(_ => SetSelectedStatus(InstallTaskStatus.Completed), _ => SelectedTask is not null && !IsReadOnly);
        GoUpCommand = new RelayCommand(_ => GoUpResourceDirectory(), _ => CanGoUpResourceDirectory);

        var location = _resourceLocator.Locate();
        ApplyResourceLocation(location);
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public ObservableCollection<InstallTask> Tasks { get; }

    public ObservableCollection<ResourceEntry> ResourceEntries { get; }

    public RelayCommand NewTaskCommand { get; }

    public RelayCommand DeleteTaskCommand { get; }

    public RelayCommand CopyTaskCommand { get; }

    public RelayCommand CutTaskCommand { get; }

    public RelayCommand PasteTaskCommand { get; }

    public RelayCommand MarkInProgressCommand { get; }

    public RelayCommand MarkCompletedCommand { get; }

    public RelayCommand GoUpCommand { get; }

    public InstallTask? SelectedTask
    {
        get => _selectedTask;
        set
        {
            if (ReferenceEquals(_selectedTask, value))
            {
                return;
            }

            _selectedTask = value;
            OnPropertyChanged();
            RefreshCommands();
        }
    }

    public ResourceEntry? SelectedResource
    {
        get => _selectedResource;
        set
        {
            if (ReferenceEquals(_selectedResource, value))
            {
                return;
            }

            _selectedResource = value;
            OnPropertyChanged();
            PreviewText = value is null ? "选择要预览的文件。" : _resourceBrowser.Preview(value);
        }
    }

    public string RootAddress => _resourceRoot is null ? "资源库未定位" : $"资源库 · {_resourceRoot}";

    public string CurrentDirectoryDisplay => _resourceRoot is null || _currentResourceDirectory is null
        ? "资源库未定位"
        : $"资源库\\{Path.GetRelativePath(_resourceRoot, _currentResourceDirectory)}".TrimEnd('\\');

    public string PreviewText
    {
        get => _previewText;
        private set
        {
            if (_previewText == value)
            {
                return;
            }

            _previewText = value;
            OnPropertyChanged();
        }
    }

    public bool CanExecuteSelectedResource => SelectedResource is not null &&
        !SelectedResource.IsDirectory &&
        _resourceExecutionService.CanExecute(SelectedResource.FullPath);

    public string ExecuteSelectedResource()
    {
        if (SelectedResource is null)
        {
            return "未选择资源。";
        }

        _resourceExecutionService.TryStart(SelectedResource.FullPath, out var message);
        StatusMessage = message;
        return message;
    }
    public string StatusMessage
    {
        get => _statusMessage;
        private set
        {
            if (_statusMessage == value)
            {
                return;
            }

            _statusMessage = value;
            OnPropertyChanged();
        }
    }

    public bool IsReadOnly => !_taskStore.IsWritable;

    public bool HasResourceLibrary => _resourceRoot is not null;

    public bool CanGoUpResourceDirectory => _resourceRoot is not null &&
        _currentResourceDirectory is not null &&
        !string.Equals(Path.GetFullPath(_resourceRoot), Path.GetFullPath(_currentResourceDirectory), StringComparison.OrdinalIgnoreCase);

    public void SelectResourceLibrary(string directory)
    {
        var location = _resourceLocator.SaveSelection(directory);
        ApplyResourceLocation(location);
        StatusMessage = $"已选择资源库：{location.RelativePath}";
    }

    public void OpenSelectedResource()
    {
        if (SelectedResource is null)
        {
            return;
        }

        if (SelectedResource.IsDirectory)
        {
            _currentResourceDirectory = SelectedResource.FullPath;
            LoadResourceEntries();
            OnPropertyChanged(nameof(CurrentDirectoryDisplay));
            GoUpCommand.Refresh();
            return;
        }

        PreviewText = _resourceBrowser.Preview(SelectedResource);
    }

    public void RenameSelectedTask(string title)
    {
        if (IsReadOnly || SelectedTask is null || string.IsNullOrWhiteSpace(title))
        {
            return;
        }

        SelectedTask.Title = title.Trim();
        SelectedTask.UpdatedAt = DateTimeOffset.UtcNow;
        SaveTasks("任务名称已更新。");
    }

    public void SortTasks()
    {
        var ordered = Tasks.OrderBy(task => task.Priority).ThenBy(task => task.Title, StringComparer.OrdinalIgnoreCase).ToArray();
        Tasks.Clear();
        foreach (var task in ordered)
        {
            Tasks.Add(task);
        }
    }

    public void GoUpResourceDirectory()
    {
        if (!CanGoUpResourceDirectory || _currentResourceDirectory is null)
        {
            return;
        }

        _currentResourceDirectory = Directory.GetParent(_currentResourceDirectory)?.FullName ?? _resourceRoot;
        LoadResourceEntries();
        OnPropertyChanged(nameof(CurrentDirectoryDisplay));
        GoUpCommand.Refresh();
    }

    private void AddTask()
    {
        if (IsReadOnly)
        {
            StatusMessage = "当前目录不可写，任务编辑已禁用。";
            return;
        }

        var task = new InstallTask { Title = "新装机任务", Priority = 1 };
        Tasks.Add(task);
        SelectedTask = task;
        SaveTasks("已新建任务。");
    }

    private void DeleteSelectedTask()
    {
        if (SelectedTask is null || IsReadOnly)
        {
            return;
        }

        Tasks.Remove(SelectedTask);
        SelectedTask = Tasks.FirstOrDefault();
        SaveTasks("已删除任务条目。");
    }

    private void CopySelectedTask()
    {
        if (SelectedTask is null)
        {
            return;
        }

        _clipboardTask = CloneTask(SelectedTask);
        _clipboardIsCut = false;
        StatusMessage = "已复制任务条目。";
        PasteTaskCommand.Refresh();
    }

    private void CutSelectedTask()
    {
        if (SelectedTask is null || IsReadOnly)
        {
            return;
        }

        _clipboardTask = SelectedTask;
        _clipboardIsCut = true;
        StatusMessage = "已剪切任务条目，等待粘贴。";
        PasteTaskCommand.Refresh();
    }

    private void PasteTask()
    {
        if (_clipboardTask is null || IsReadOnly)
        {
            return;
        }

        if (_clipboardIsCut)
        {
            if (!Tasks.Contains(_clipboardTask))
            {
                Tasks.Add(_clipboardTask);
            }
        }
        else
        {
            var clone = CloneTask(_clipboardTask);
            Tasks.Add(clone);
            SelectedTask = clone;
        }

        SaveTasks("已粘贴任务条目。");
        _clipboardTask = null;
        _clipboardIsCut = false;
        PasteTaskCommand.Refresh();
    }

    private void SetSelectedStatus(InstallTaskStatus status)
    {
        if (SelectedTask is null || IsReadOnly)
        {
            return;
        }

        SelectedTask.Status = status;
        SelectedTask.UpdatedAt = DateTimeOffset.UtcNow;
        SaveTasks($"任务状态已更新为 {status}。");
    }

    private void ApplyResourceLocation(ResourceLibraryLocation location)
    {
        _resourceRoot = location.FullPath;
        _currentResourceDirectory = location.FullPath;
        LoadResourceEntries();
        OnPropertyChanged(nameof(RootAddress));
        OnPropertyChanged(nameof(CurrentDirectoryDisplay));
        OnPropertyChanged(nameof(HasResourceLibrary));
        OnPropertyChanged(nameof(CanGoUpResourceDirectory));
        GoUpCommand.Refresh();
        StatusMessage = location.IsAvailable ? "资源库已加载。" : "未找到资源库，请选择目录。";
    }

    private void LoadResourceEntries()
    {
        ResourceEntries.Clear();
        if (_currentResourceDirectory is null)
        {
            return;
        }

        foreach (var entry in _resourceBrowser.List(_currentResourceDirectory))
        {
            ResourceEntries.Add(entry);
        }
    }

    private void SaveTasks(string message)
    {
        try
        {
            _taskStore.Save(Tasks);
            StatusMessage = message;
        }
        catch (IOException exception)
        {
            StatusMessage = $"任务保存失败：{exception.Message}";
        }
        catch (UnauthorizedAccessException)
        {
            StatusMessage = "任务保存失败：访问被拒绝。";
        }
    }

    private void RefreshCommands()
    {
        DeleteTaskCommand.Refresh();
        CopyTaskCommand.Refresh();
        CutTaskCommand.Refresh();
        MarkInProgressCommand.Refresh();
        MarkCompletedCommand.Refresh();
    }

    private static InstallTask CloneTask(InstallTask task)
    {
        return new InstallTask
        {
            Title = $"{task.Title} - 副本",
            Status = task.Status,
            Priority = task.Priority,
            Category = task.Category,
            Notes = task.Notes
        };
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
}
