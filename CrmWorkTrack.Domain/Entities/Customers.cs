using System.ComponentModel.DataAnnotations.Schema;

namespace CrmWorkTrack.Domain.Entities;

public class Customer
{
    public int Id { get; set; }
    public string CompanyName { get; set; } = null!;
    public string? ContactName { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;
    public int? UpdatedByUserId { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeletedAt { get; set; }
    public int? DeletedByUserId { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public ICollection<Job> Jobs { get; set; } = new List<Job>();
    public ICollection<CustomerContact> Contacts { get; set; } = new List<CustomerContact>();
}

