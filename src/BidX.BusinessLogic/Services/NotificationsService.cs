using System.Diagnostics;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.NotificationDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Events;
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

    public async Task MarkAllNotificationsAsRead(int callerId)
    {
        await appDbContext.NotificationRecipients
            .Where(nr => nr.RecipientId == callerId && !nr.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(nr => nr.IsRead, true));
    }

    public async Task NotifyUserWithUnreadNotificationsCount(int userId)
    {
        var unreadNotificationsCount = await appDbContext.NotificationRecipients
            .CountAsync(nr => nr.RecipientId == userId && !nr.IsRead);

        if (unreadNotificationsCount > 0)
            await realTimeService.NotifyUserWithUnreadNotificationsCount(userId, unreadNotificationsCount);
    }

    public async Task SendPlacedBidNotifications(BidPlacedEvent evt)
    {
        var notifications = new List<Notification>();

        // Notification for auctioneer
        notifications.Add(new Notification
        {
            Message = $"**{{issuerName}}** placed a bid of **{evt.BidAmount} EGP** on your **{evt.AuctionProductName}** auction", // **X** X will be formatted as bold in frontend
            RedirectTo = RedirectTo.AuctionPage,
            RedirectId = evt.AuctionId,
            IssuerId = evt.BidderId,
            CreatedAt = DateTimeOffset.UtcNow,
            NotificationRecipients = [new() { RecipientId = evt.AuctioneerId, EventId = evt.Id }]
        });


        // Notification for previous highest bidder if exists
        if (evt.PreviousHighBidderId.HasValue && evt.PreviousHighBidderId.Value != evt.BidderId)
        {
            notifications.Add(new Notification
            {
                Message = $"**{{issuerName}}** outbid you with **{evt.BidAmount} EGP** on **{evt.AuctionProductName}** auction",
                RedirectTo = RedirectTo.AuctionPage,
                RedirectId = evt.AuctionId,
                IssuerId = evt.BidderId,
                CreatedAt = DateTimeOffset.UtcNow,
                NotificationRecipients = [new() { RecipientId = evt.PreviousHighBidderId.Value, EventId = evt.Id }]
            });
        }

        try
        {
            await SaveNotifications(notifications);
            await NotifyUsersThatTheyGotNotification(notifications);
        }
        catch (DbUpdateException ex) when (IsUniqueConstraintViolation(ex))
        {
            // Skip if notifications for this event were already processed
            // This ensures idempotency and prevents duplicate notifications
            return;
        }
    }

    public async Task SendAcceptedBidNotifications(int auctionId, string auctionTitle, int winnerId, int auctioneerId, IEnumerable<int> bidderIds)
    {
        var notifications = new List<Notification>();

        // Notification for the winner
        notifications.Add(new Notification
        {
            Message = $"Congratulations! Your bid on **{auctionTitle}** has been accepted by **{{issuerName}}**",
            RedirectTo = RedirectTo.AuctionPage,
            RedirectId = auctionId,
            IssuerId = auctioneerId,
            CreatedAt = DateTimeOffset.UtcNow,
            NotificationRecipients = [new() { RecipientId = winnerId }]
        });

        // Notification for other bidders
        var otherBidders = bidderIds
            .Distinct()
            .Where(id => id != winnerId)
            .Select(id => new NotificationRecipient { RecipientId = id })
            .ToList();

        notifications.Add(new Notification
        {
            Message = $"Better luck next time! **{{issuerName}}** won the **{auctionTitle}** auction",
            RedirectTo = RedirectTo.AuctionPage,
            RedirectId = auctionId,
            IssuerId = winnerId,
            CreatedAt = DateTimeOffset.UtcNow,
            NotificationRecipients = otherBidders
        });

        await SaveNotifications(notifications);
        await NotifyUsersThatTheyGotNotification(notifications);
    }


    private async Task SaveNotifications(IEnumerable<Notification> notifications)
    {
        appDbContext.Notifications.AddRange(notifications);
        await appDbContext.SaveChangesAsync();
    }

    private async Task NotifyUsersThatTheyGotNotification(IEnumerable<Notification> notifications)
    {
        var uniqueRecipientIds = notifications
            .SelectMany(n => n.NotificationRecipients!)
            .Select(nr => nr.RecipientId)
            .Distinct()
            .ToList();

        var unreadCountsMap = await GetUnreadNotificationsCounts(uniqueRecipientIds);

        var tasks = uniqueRecipientIds
            .Select(recipientId =>
                realTimeService.NotifyUserWithUnreadNotificationsCount(
                    recipientId,
                    unreadCountsMap[recipientId]
                )
            );

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

    private bool IsUniqueConstraintViolation(DbUpdateException ex)
    {
        // Check if the database exception is due to a unique constraint violation
        // This occurs when trying to insert a duplicate EventId + RecipientId combination

        return ex.InnerException?.Message?.Contains("duplicate") ?? false;
    }
}
