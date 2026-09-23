using TaskDesk.Models;
using TaskDesk.Services;

namespace TaskDesk.Tests;

public sealed class SmokeTests
{
    [Xunit.Fact]
    public void TaskStore_CreatesAndReloadsTasks()
    {
        var root = Path.Combine(Path.GetTempPath(), "TaskDeskTests", Guid.NewGuid().ToString("N"));
        try
        {
            var paths = new TaskDeskPaths(root);
            var store = new TaskStore(paths);
            var task = new InstallTask { Title = "安装显卡驱动", Priority = 1 };

            Xunit.Assert.True(store.IsWritable);
            store.Save([task]);

            var loaded = store.Load();
            Xunit.Assert.Single(loaded);
            Xunit.Assert.Equal(task.Title, loaded[0].Title);
            Xunit.Assert.Equal(InstallTaskStatus.Pending, loaded[0].Status);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Xunit.Fact]
    public void ResourceLibraryLocator_PrefersConfiguredRelativeDirectory()
    {
        var root = Path.Combine(Path.GetTempPath(), "TaskDeskTests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "资源库"));
            Directory.CreateDirectory(Path.Combine(root, "自定义资源"));
            var paths = new TaskDeskPaths(root);
            var config = new ConfigStore(paths);
            config.SaveResourceLibraryPath("自定义资源");

            var location = new ResourceLibraryLocator(paths, config).Locate();

            Xunit.Assert.True(location.IsAvailable);
            Xunit.Assert.Equal(ResourceLibrarySource.Config, location.Source);
            Xunit.Assert.Equal("自定义资源", location.RelativePath);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }

    [Xunit.Fact]
    public void ResourceLibraryLocator_FallsBackToSiblingFolder()
    {
        var root = Path.Combine(Path.GetTempPath(), "TaskDeskTests", Guid.NewGuid().ToString("N"));
        try
        {
            Directory.CreateDirectory(Path.Combine(root, "资源库"));
            var paths = new TaskDeskPaths(root);

            var location = new ResourceLibraryLocator(paths, new ConfigStore(paths)).Locate();

            Xunit.Assert.True(location.IsAvailable);
            Xunit.Assert.Equal(ResourceLibrarySource.SiblingFolder, location.Source);
            Xunit.Assert.Equal("资源库", location.RelativePath);
        }
        finally
        {
            if (Directory.Exists(root))
            {
                Directory.Delete(root, true);
            }
        }
    }
    [Xunit.Fact]
    public void ResourceExecutionService_OnlyAllowsConfiguredExtensions()
    {
        var service = new ResourceExecutionService();
        var root = Path.Combine(Path.GetTempPath(), "TaskDeskTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(root);
        try
        {
            var executable = Path.Combine(root, "tool.exe");
            var textFile = Path.Combine(root, "notes.txt");
            File.WriteAllText(executable, string.Empty);
            File.WriteAllText(textFile, string.Empty);

            Xunit.Assert.True(service.CanExecute(executable));
            Xunit.Assert.False(service.CanExecute(textFile));
            Xunit.Assert.False(service.TryStart(Path.Combine(root, "missing.exe"), out _));
        }
        finally
        {
            Directory.Delete(root, true);
        }
    }}