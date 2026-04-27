using CrmWorkTrack.Application.DTOs;
using CrmWorkTrack.Domain.Entities;
using CrmWorkTrack.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CrmWorkTrack.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class NotificationsController : ControllerBase
{
    private readonly AppDbContext _context;

    public NotificationsController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<List<NotificationDto>>> GetAll()
    {
        var notifications = await _context.Notifications
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new NotificationDto
            {
                Id = x.Id,
                Title = x.Title,
                Message = x.Message,
                IsRead = x.IsRead,
                CreatedAt = x.CreatedAt
            })
            .ToListAsync();

        return Ok(notifications);
    }

    [HttpGet("unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        var count = await _context.Notifications.CountAsync(x => !x.IsRead);
        return Ok(count);
    }

    [HttpPost]
    public async Task<ActionResult> Create(NotificationDto dto)
    {
        var notification = new Notification
        {
            Title = dto.Title,
            Message = dto.Message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();

        return Ok(notification);
    }

    [HttpPut("{id}/read")]
    public async Task<ActionResult> MarkAsRead(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null)
            return NotFound();

        notification.IsRead = true;
        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPut("read-all")]
    public async Task<ActionResult> MarkAllAsRead()
    {
        var notifications = await _context.Notifications
            .Where(x => !x.IsRead)
            .ToListAsync();

        foreach (var notification in notifications)
        {
            notification.IsRead = true;
        }

        await _context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult> Delete(int id)
    {
        var notification = await _context.Notifications.FindAsync(id);

        if (notification == null)
            return NotFound();

        _context.Notifications.Remove(notification);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}