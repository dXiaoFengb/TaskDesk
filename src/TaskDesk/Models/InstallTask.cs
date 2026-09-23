namespace TaskDesk.Models;

public sealed class InstallTask
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Title { get; set; } = string.Empty;

    public InstallTaskStatus Status { get; set; } = InstallTaskStatus.Pending;

    public int Priority { get; set; }

    public string Category { get; set; } = string.Empty;

    public string Notes { get; set; } = string.Empty;

    public DateTimeOffset CreatedAt { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;
}
