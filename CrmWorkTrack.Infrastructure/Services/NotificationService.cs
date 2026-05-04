using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.Infrastructure.Services;

public class NotificationService
{
    private readonly AppDbContext _context;

    public NotificationService(AppDbContext context)
    {
        _context = context;
    }

    public async Task CreateAsync(string title, string message, int? userId = null)
    {
        var notification = new Notification
        {
            Title = title,
            Message = message,
            UserId = userId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task CreateJobAssignedAsync(int assignedUserId, string jobTitle)
    {
        var user = await _context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == assignedUserId);

        if (user == null)
            return;

        var notification = new Notification
        {
            Title = "Yeni İş Atandı",
            Message = $"Size yeni bir iş atandı: {jobTitle}",
            UserId = assignedUserId,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }
}