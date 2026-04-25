namespace CrmWorkTrack.Domain.Entities;

public class Permission
{
    public int Id { get; set; }
    public string Code { get; set; } = null!;

 
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }

    public ICollection<RolePermission> RolePermissions { get; set; }
        = new List<RolePermission>();
}
