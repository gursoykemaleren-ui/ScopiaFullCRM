namespace CrmWorkTrack.Domain.Entities;

public class Ticket
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = "Open";
    public string Priority { get; set; } = "Medium";

    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime? UpdatedAt { get; set; }
}