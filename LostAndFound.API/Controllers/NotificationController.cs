using LostAndFound.Application.Interfaces.Notifications;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LostAndFound.API.Controllers;

[ApiController]
[Route("api/notifications")]
[Authorize] // Tất cả user đã đăng nhập đều có thể xem thông báo của mình
public class NotificationController : ControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return int.Parse(userIdClaim ?? "0");
    }

    /// <summary>
    /// Lấy danh sách thông báo của người dùng hiện tại
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetUserNotifications([FromQuery] bool? unreadOnly = null)
    {
        var userId = GetCurrentUserId();
        var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly);
        return Ok(notifications);
    }

    /// <summary>
    /// Đếm số thông báo chưa đọc của người dùng hiện tại
    /// </summary>
    [HttpGet("unread-count")]
    public async Task<IActionResult> GetUnreadCount()
    {
        var userId = GetCurrentUserId();
        var count = await _notificationService.GetUnreadCountAsync(userId);
        return Ok(new { UnreadCount = count });
    }

    /// <summary>
    /// Đánh dấu một thông báo là đã đọc
    /// </summary>
    [HttpPut("{id}/read")]
    public async Task<IActionResult> MarkAsRead(int id)
    {
        var userId = GetCurrentUserId();
        var result = await _notificationService.MarkAsReadAsync(id, userId);
        if (!result)
        {
            return NotFound(new { Message = "Không tìm thấy thông báo hoặc bạn không có quyền." });
        }
        return NoContent();
    }
}

