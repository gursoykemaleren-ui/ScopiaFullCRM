namespace CrmWorkTrack.Domain.Entities;

public class Attachment
{
    public int Id { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string OriginalFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long Size { get; set; }

    public string FilePath { get; set; } = string.Empty;

    public int? JobId { get; set; }
    public Job? Job { get; set; }

    public int? TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public int? CustomerId { get; set; }
    public Customer? Customer { get; set; }

    public int? UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}