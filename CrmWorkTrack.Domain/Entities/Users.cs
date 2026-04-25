namespace CrmWorkTrack.Domain.Entities;

public class User
{

    public int Id { get; set; }
    public string Email { get; set; } = null!;
    public string UserName { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? UniqueKey { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    // Navigation
    public ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public ICollection<Job> CreatedJobs { get; set; } = new List<Job>();
    public ICollection<Job> AssignedJobs { get; set; } = new List<Job>();
}
