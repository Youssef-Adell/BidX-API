using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.DTOs.NotificationDTOs;

public class NotificationResponse
{
    public int Id { get; set; }
    public string Message { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public RedirectTo RedirectTo { get; set; }
    public int? RedirectId { get; set; }
    public bool IsRead { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}