namespace CrmWorkTrack.Domain.Entities;

public class JobActivity
{
    public int Id { get; set; }

    public int? JobId { get; set; }
    public Job? Job { get; set; } = null!;

    public string Type { get; set; } = null!;        // "created", "status_changed", "assigned", "comment_added", ...
    public string? Message { get; set; }             // okunabilir açıklama
    public string? MetaJson { get; set; }            // opsiyonel: {"from":"Open","to":"Done"} gibi

    public int? PerformedByUserId { get; set; }
    public User? PerformedByUser { get; set; }

    public DateTime CreatedAt { get; set; }
}
