using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.NotificationDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Extensions;
using BidX.BusinessLogic.Interfaces;
using BidX.DataAccess;
using BidX.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidX.BusinessLogic.Services;

public class NotificationsService : INotificationsService
{
    private readonly AppDbContext appDbContext;
    private readonly IRealTimeService realTimeService;

    public NotificationsService(AppDbContext appDbContext, IRealTimeService realTimeService)
    {
        this.appDbContext = appDbContext;
        this.realTimeService = realTimeService;
    }


    public async Task<Page<NotificationResponse>> GetUserNotifications(int userId, NotificationsQueryParams queryParams)
    {
        // Build the query based on the parameters
        var query = appDbContext.NotificationRecipients.Where(nr => nr.RecipientId == userId);

        // Get the total count before pagination
        var totalCount = await query.CountAsync();
        if (totalCount == 0)
            return new Page<NotificationResponse>([], queryParams.Page, queryParams.PageSize, totalCount);

        // Get the list of notifications with pagination and projection
        var notifications = await query
            .OrderByDescending(nr => nr.NotificationId)
            .ProjectToNotificationResponse()
            .Paginate(queryParams.Page, queryParams.PageSize)
            .ToListAsync();

        return new Page<NotificationResponse>(notifications, queryParams.Page, queryParams.PageSize, totalCount);
    }

    public async Task MarkNotificationAsRead(int callerId, int notificationId)
    {
        await appDbContext.NotificationRecipients
            .Where(nr => nr.RecipientId == callerId && nr.NotificationId == notificationId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(nr => nr.IsRead, true));
    }

    public async Task NotifyUserWithUnreadNotificationsCount(int userId)
    {
        var unreadNotificationsCount = await appDbContext.NotificationRecipients
            .CountAsync(nr => nr.RecipientId == userId && !nr.IsRead);

        await realTimeService.NotifyUserWithUnreadNotificationsCount(userId, unreadNotificationsCount);
    }

    public async Task NotifyNewBid(int auctionId, string auctionTitle, decimal bidAmount, int bidderId, int auctioneerId, int? previousHighestBidderId)
    {
        var notifications = new List<Notification>();

        // Notification for auctioneer
        notifications.Add(new Notification
        {
            Message = $"**{{userName}}** placed a bid of **{bidAmount} EGP** on your auction **{auctionTitle}**", // **X** X will be formatted as bold in frontend
            RedirectTo = RedirectTo.AuctionPage,
            RedirectId = auctionId,
            IssuerId = bidderId,
            CreatedAt = DateTimeOffset.UtcNow,
            NotificationRecipients = [new() { RecipientId = auctioneerId }]
        });


        // Notification for previous highest bidder if exists
        if (previousHighestBidderId.HasValue && previousHighestBidderId.Value != bidderId)
        {
            notifications.Add(new Notification
            {
                Message = $"**{{userName}}** outbid you with **{bidAmount} EGP** on auction **{auctionTitle}**",
                RedirectTo = RedirectTo.AuctionPage,
                RedirectId = auctionId,
                IssuerId = bidderId,
                CreatedAt = DateTimeOffset.UtcNow,
                NotificationRecipients = [new() { RecipientId = previousHighestBidderId.Value }]
            });
        }

        await BulkInsertNotifications(notifications);
        await SendRealTimeNotifications(notifications);
    }

    private async Task BulkInsertNotifications(IEnumerable<Notification> notifications)
    {
        appDbContext.Notifications.AddRange(notifications);
        await appDbContext.SaveChangesAsync();
    }

    private async Task SendRealTimeNotifications(IEnumerable<Notification> notifications)
    {
        // Get unique recipient IDs to avoid duplicate queries
        var uniqueRecipientIds = notifications
            .SelectMany(n => n.NotificationRecipients!)
            .Select(nr => nr.RecipientId)
            .Distinct()
            .ToList();

        // Single database query to get all unread counts
        var unreadCountsMap = await GetUnreadNotificationsCounts(uniqueRecipientIds);

        var tasks = new List<Task>();

        foreach (var notification in notifications)
        {
            tasks.AddRange(notification.NotificationRecipients!
                .Select(nr =>
                    realTimeService.NotifyUserWithUnreadNotificationsCount(
                        nr.RecipientId,
                        unreadCountsMap[nr.RecipientId]
                    )
                ));
        }

        await Task.WhenAll(tasks);
    }


    private async Task<Dictionary<int, int>> GetUnreadNotificationsCounts(ICollection<int> userIds)
    {
        // Single database query to get counts for all users
        var unreadCounts = await appDbContext.NotificationRecipients
            .Where(nr => userIds.Contains(nr.RecipientId) && !nr.IsRead)
            .GroupBy(nr => nr.RecipientId)
            .Select(g => new
            {
                UserId = g.Key,
                Count = g.Count()
            })
            .ToDictionaryAsync(x => x.UserId, x => x.Count);

        // Ensure all users have an entry, even if they have no unread notifications
        foreach (var userId in userIds)
        {
            if (!unreadCounts.ContainsKey(userId))
            {
                unreadCounts[userId] = 0;
            }
        }

        return unreadCounts;
    }

}
