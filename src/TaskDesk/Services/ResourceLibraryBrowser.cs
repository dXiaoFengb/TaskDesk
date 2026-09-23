using System.IO;
using System.Text;
using TaskDesk.Models;

namespace TaskDesk.Services;

public sealed class ResourceLibraryBrowser
{
    public IReadOnlyList<ResourceEntry> List(string directory)
    {
        if (!Directory.Exists(directory))
        {
            return [];
        }

        var entries = new List<ResourceEntry>();
        foreach (var item in new DirectoryInfo(directory).EnumerateFileSystemInfos())
        {
            if ((item.Attributes & FileAttributes.Hidden) != 0)
            {
                continue;
            }

            var isDirectory = item is DirectoryInfo;
            var size = isDirectory ? 0 : ((FileInfo)item).Length;
            var type = isDirectory ? "文件夹" : GetTypeName(item.Extension);
            entries.Add(new ResourceEntry(item.Name, item.FullName, isDirectory, item.LastWriteTime, type, size));
        }

        return entries.OrderByDescending(entry => entry.IsDirectory).ThenBy(entry => entry.Name, StringComparer.OrdinalIgnoreCase).ToArray();
    }

    public string Preview(ResourceEntry entry)
    {
        if (entry.IsDirectory)
        {
            return "双击文件夹可进入，选择文件后显示预览。";
        }

        if (!string.Equals(Path.GetExtension(entry.FullPath), ".txt", StringComparison.OrdinalIgnoreCase) &&
            !string.Equals(Path.GetExtension(entry.FullPath), ".md", StringComparison.OrdinalIgnoreCase))
        {
            return "当前首版仅预览 TXT 和 MD 文本文件。";
        }

        try
        {
            using var reader = new StreamReader(entry.FullPath, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
            return reader.ReadToEnd();
        }
        catch (IOException exception)
        {
            return $"无法读取文件：{exception.Message}";
        }
        catch (UnauthorizedAccessException)
        {
            return "无法读取文件：访问被拒绝。";
        }
    }

    private static string GetTypeName(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".txt" => "文本文档",
            ".md" => "Markdown 源文件",
            ".exe" => "应用程序",
            ".bat" or ".cmd" or ".ps1" => "脚本文件",
            ".lnk" => "快捷方式",
            _ => string.IsNullOrWhiteSpace(extension) ? "文件" : $"{extension.TrimStart('.').ToUpperInvariant()} 文件"
        };
    }
}
