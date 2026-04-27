namespace CrmWorkTrack.Domain.Entities;

public class CustomerInteraction
{
    public int Id { get; set; }

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int? CreatedByUserId { get; set; }
    public User? CreatedByUser { get; set; }

    public string InteractionType { get; set; } = "Other";
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime InteractionDate { get; set; } = DateTime.Now;
    public DateTime CreatedAt { get; set; } = DateTime.Now;
}