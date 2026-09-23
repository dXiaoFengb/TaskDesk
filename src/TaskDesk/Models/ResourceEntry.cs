namespace TaskDesk.Models;

public sealed record ResourceEntry(
    string Name,
    string FullPath,
    bool IsDirectory,
    DateTimeOffset ModifiedAt,
    string Type,
    long Size)
{
    public string DisplaySize => IsDirectory ? string.Empty : $"{Size:N0} B";
}
