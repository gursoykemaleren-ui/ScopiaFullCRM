using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;

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
}