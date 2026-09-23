using System.Diagnostics;
using System.IO;

namespace TaskDesk.Services;

public sealed class ResourceExecutionService
{
    private static readonly HashSet<string> SupportedExtensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".exe", ".bat", ".cmd", ".ps1", ".lnk"
    };

    public bool CanExecute(string path) =>
        File.Exists(path) && SupportedExtensions.Contains(Path.GetExtension(path));

    public bool TryStart(string path, out string message)
    {
        if (!CanExecute(path))
        {
            message = "该文件类型不在允许启动范围内。";
            return false;
        }

        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = path,
                WorkingDirectory = Path.GetDirectoryName(path) ?? string.Empty,
                UseShellExecute = true,
                Verb = "open"
            };
            Process.Start(startInfo);
            message = "已按普通用户权限请求启动资源。";
            return true;
        }
        catch (Exception exception) when (exception is IOException or UnauthorizedAccessException or InvalidOperationException)
        {
            message = $"启动失败：{exception.Message}";
            return false;
        }
    }
}