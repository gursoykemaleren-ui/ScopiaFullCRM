using CrmWorkTrack.Domain.Enums;

namespace CrmWorkTrack.Domain.Entities;

public class Job
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;
    public string? Description { get; set; }

    public bool IsCompleted { get; set; }

    public JobStatus Status { get; set; } = JobStatus.Open;

    public int CustomerId { get; set; }
    public Customer Customer { get; set; } = null!;

    public int CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    public int? AssignedToUserId { get; set; }
    public User? AssignedToUser { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string Priority { get; set; } = "Medium";
    public DateTime? DueDate { get; set; }

    public bool IsActive { get; set; } = true;

    public DateTime? UpdatedAt { get; set; }

    public int? UpdatedByUserId { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }

}

