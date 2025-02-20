using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.BusinessLogic.DTOs.CommonDTOs;
using BidX.BusinessLogic.DTOs.QueryParamsDTOs;
using BidX.BusinessLogic.Extensions;
using BidX.BusinessLogic.Interfaces;
using BidX.BusinessLogic.Mappings;
using BidX.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace BidX.BusinessLogic.Services;

public class ChatsService : IChatsService
{
    private readonly AppDbContext appDbContext;
    private readonly IRealTimeService realTimeService;

    public ChatsService(AppDbContext appDbContext, IRealTimeService realTimeService)
    {
        this.appDbContext = appDbContext;
        this.realTimeService = realTimeService;
    }

    public async Task<Page<ChatDetailsResponse>> GetUserChats(int userId, ChatsQueryParams queryParams)
    {
        // Build the query based on userId parameter
        var userChatsQuery = appDbContext.Chats.Where(c => c.Participant1Id == userId || c.Participant2Id == userId);

        // Get the total count before pagination
        var totalCount = await userChatsQuery.CountAsync();
        if (totalCount == 0)
            return new Page<ChatDetailsResponse>([], queryParams.Page, queryParams.PageSize, totalCount);

        // Get the list of chats with pagination and mapping
        var userChats = await userChatsQuery
            .OrderByDescending(c => c.LastMessageId)
            .ProjectToChatDetailsResponse(userId)
            .Paginate(queryParams.Page, queryParams.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return new Page<ChatDetailsResponse>(userChats, queryParams.Page, queryParams.PageSize, totalCount);
    }

    public async Task<Result<ChatSummeryResponse>> CreateChatOrGetIfExist(int callerId, CreateChatRequest request)
    {
        var existingChat = await GetChatIfExist(callerId, request.ParticipantId);

        if (existingChat is not null)
            return Result<ChatSummeryResponse>.Success(existingChat);

        return await CreateChat(callerId, request);
    }

    public async Task<Result<ChatSummeryResponse>> GetChat(int callerId, int chatId)
    {
        var chat = await appDbContext.Chats
             .Where(c => c.Id == chatId && (c.Participant1Id == callerId || c.Participant2Id == callerId))
             .ProjectToChatSummaryResponse(callerId)
             .FirstOrDefaultAsync();

        if (chat is null)
            return Result<ChatSummeryResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Chat not found."]);

        return Result<ChatSummeryResponse>.Success(chat);
    }

    public async Task<Result<Page<MessageResponse>>> GetChatMessages(int callerId, int chatId, MessagesQueryParams queryParams)
    {
        // Build the query based on parameters
        var chatMessagesQuery = appDbContext.Messages
            .Where(m => m.ChatId == chatId && (m.SenderId == callerId || m.RecipientId == callerId));

        // Get the total count before pagination
        var totalCount = await chatMessagesQuery.CountAsync();
        if (totalCount == 0)
        {
            var chatExists = await appDbContext.Chats.AnyAsync(c => c.Participant1Id == callerId || c.Participant2Id == callerId);
            if (!chatExists)
                return Result<Page<MessageResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Chat not found."]);

            return Result<Page<MessageResponse>>.Success(new Page<MessageResponse>([], queryParams.Page, queryParams.PageSize, totalCount));
        }

        // Get the list of messages with pagination and mapping
        var chatMessages = await chatMessagesQuery
            .OrderByDescending(c => c.Id)
            .ProjectToMessageResponse()
            .Paginate(queryParams.Page, queryParams.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var page = new Page<MessageResponse>(chatMessages, queryParams.Page, queryParams.PageSize, totalCount);
        return Result<Page<MessageResponse>>.Success(page);
    }

    public async Task SendMessage(int senderId, SendMessageRequest request)
    {
        var chat = await appDbContext.Chats
            .Where(c => c.Id == request.ChatId && (c.Participant1Id == senderId || c.Participant2Id == senderId))
            .Select(c => new { ParticipantId = c.Participant1Id == senderId ? c.Participant2Id : c.Participant1Id })
            .FirstOrDefaultAsync();

        if (chat is null)
        {
            await realTimeService.SendErrorToUser(senderId, ErrorCode.RESOURCE_NOT_FOUND, ["Chat not found."]);
            return;
        }

        // Create and save the message
        var message = request.ToMessageEntity(senderId, chat.ParticipantId);
        appDbContext.Messages.Add(message);
        await appDbContext.SaveChangesAsync();

        // Send the message to the chat
        var response = message.ToMessageResponse();
        await Task.WhenAll(
            realTimeService.SendMessageToChat(response.ChatId, response),
            NotifyUserWithUnreadChatsCount(response.RecipientId)
        );
    }

    public async Task MarkMessageAsRead(int readerId, MarkMessageAsReadRequest request)
    {
        var message = await appDbContext.Messages
            .Where(m => m.Id == request.MessageId && m.RecipientId == readerId)
            .Select(m => new { m.Id, m.ChatId })
            .FirstOrDefaultAsync();

        if (message == null)
        {
            await realTimeService.SendErrorToUser(readerId, ErrorCode.RESOURCE_NOT_FOUND, ["Message not found."]);
            return;
        }

        await appDbContext.Messages
            .Where(m => m.Id == request.MessageId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.IsRead, true));

        await realTimeService.MarkMessageAsRead(message.ChatId, message.Id);
    }

    public async Task MarkAllMessagesAsRead(int readerId, MarkAllMessagesAsReadRequest request)
    {
        await appDbContext.Messages
            .Where(m => m.ChatId == request.ChatId && m.RecipientId == readerId && !m.IsRead)
            .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.IsRead, true));

        await realTimeService.MarkAllMessagesAsRead(request.ChatId, readerId);
    }

    public async Task NotifyUserWithUnreadChatsCount(int userId)
    {
        var unreadChatsCount = await appDbContext.Messages
            .Where(m => m.RecipientId == userId && m.IsRead == false)
            .GroupBy(m => m.ChatId)
            .CountAsync();

        if (unreadChatsCount > 0)
            await realTimeService.NotifyUserWithUnreadChatsCount(userId, unreadChatsCount);
    }

    public async Task NotifyParticipantsWithUserStatus(int userId, bool isOnline)
    {
        await appDbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.IsOnline, isOnline));

        var chatIdsToNotify = await appDbContext.Chats
            .Where(c => c.Participant1Id == userId || c.Participant2Id == userId)
            .AsNoTracking()
            .Select(c => c.Id)
            .ToListAsync();

        await realTimeService.NotifyParticipantsWithUserStatus(chatIdsToNotify, userId, isOnline);
    }


    private async Task<ChatSummeryResponse?> GetChatIfExist(int participant1Id, int participant2Id)
    {
        return await appDbContext.Chats
            .Where(c => (c.Participant1Id == participant1Id && c.Participant2Id == participant2Id)
                || (c.Participant1Id == participant2Id && c.Participant2Id == participant1Id))
            .ProjectToChatSummaryResponse(participant1Id)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    private async Task<Result<ChatSummeryResponse>> CreateChat(int callerId, CreateChatRequest request)
    {
        // Check chatting eligibility
        var hasDealtWithUserBefore = await appDbContext.Auctions.AnyAsync(a =>
            (a.WinnerId == callerId && a.AuctioneerId == request.ParticipantId) ||
            (a.AuctioneerId == callerId && a.WinnerId == request.ParticipantId));

        if (!hasDealtWithUserBefore)
            return Result<ChatSummeryResponse>.Failure(ErrorCode.CHATTING_NOT_ALLOWED, ["You cannot chat with a user you have not dealt with before."]);

        var participant = await appDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.ParticipantId);

        if (participant == null)
            return Result<ChatSummeryResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Participant not found."]);

        // Save the chat into the the database
        var chat = request.ToChatEntity(callerId);
        appDbContext.Chats.Add(chat);
        await appDbContext.SaveChangesAsync();

        var response = chat.ToChatSummeryResponse(callerId, participant);
        return Result<ChatSummeryResponse>.Success(response);
    }
}