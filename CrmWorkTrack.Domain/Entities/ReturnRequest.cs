namespace CrmWorkTrack.Domain.Entities;

public class ReturnRequest
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; }

    public int? JobId { get; set; }
    public Job? Job { get; set; }

    public int? TicketId { get; set; }
    public Ticket? Ticket { get; set; }

    public string Reason { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Pending";
    // Pending | Approved | Rejected

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}