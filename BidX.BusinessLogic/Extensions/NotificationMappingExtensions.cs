using BidX.BusinessLogic.DTOs.NotificationDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Extensions;

public static class NotificationMappingExtensions
{
    public static NotificationResponse ToNotificationResponse(this Notification notification)
    {
        return new NotificationResponse
        {
            Id = notification.Id,
            Message = notification.Message,
            ThumbnailUrl = notification.Issuer!.ProfilePictureUrl,
            RedirectTo = notification.RedirectTo,
            RedirectId = notification.RedirectId,
            IsRead = notification.IsRead,
            CreatedAt = notification.CreatedAt
        };
    }

    public static IQueryable<NotificationResponse> ProjectToNotificationResponse(this IQueryable<NotificationRecipient> query)
    {
        return query.Select(nr => new NotificationResponse
        {
            Id = nr.NotificationId,
            Message = nr.Notification!.Message.Replace("{issuerName}", nr.Notification.Issuer!.FullName),
            ThumbnailUrl = nr.Notification.Issuer.ProfilePictureUrl,
            RedirectTo = nr.Notification.RedirectTo,
            RedirectId = nr.Notification.RedirectId,
            IsRead = nr.IsRead,
            CreatedAt = nr.Notification.CreatedAt
        });
    }
}
