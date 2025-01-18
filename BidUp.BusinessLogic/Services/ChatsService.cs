using AutoMapper;
using AutoMapper.QueryableExtensions;
using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class ChatsService : IChatsService
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;

    public ChatsService(AppDbContext appDbContext, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.mapper = mapper;

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
            .ProjectTo<ChatDetailsResponse>(mapper.ConfigurationProvider, new { userId })
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .AsNoTracking()
            .ToListAsync();

        return new Page<ChatDetailsResponse>(userChats, queryParams.Page, queryParams.PageSize, totalCount);
    }

    public async Task<AppResult<ChatSummeryResponse>> CreateChatOrGetIfExist(int callerId, CreateChatRequest request)
    {
        var existingChat = await GetChatIfExist(callerId, request.ParticipantId);

        if (existingChat is not null)
            return AppResult<ChatSummeryResponse>.Success(existingChat);

        return await CreateChat(callerId, request);
    }

    public async Task<AppResult<Page<MessageResponse>>> GetChatMessages(int callerId, int chatId, MessagesQueryParams queryParams)
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
                return AppResult<Page<MessageResponse>>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Chat not found."]);

            return AppResult<Page<MessageResponse>>.Success(new Page<MessageResponse>([], queryParams.Page, queryParams.PageSize, totalCount));
        }

        // Get the list of messages with pagination and mapping
        var chatMessages = await chatMessagesQuery
            .OrderByDescending(c => c.Id)
            .ProjectTo<MessageResponse>(mapper.ConfigurationProvider)
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .AsNoTracking()
            .ToListAsync();

        await MarkReceivedMessagesAsRead(callerId, chatId);

        var page = new Page<MessageResponse>(chatMessages, queryParams.Page, queryParams.PageSize, totalCount);
        return AppResult<Page<MessageResponse>>.Success(page);
    }

    public async Task<AppResult<MessageResponse>> SendMessage(int senderId, SendMessageRequest request)
    {
        var chat = await appDbContext.Chats
            .Where(c => c.Id == request.ChatId && (c.Participant1Id == senderId || c.Participant2Id == senderId))
            .Select(c => new { ParticipantId = c.Participant1Id == senderId ? c.Participant2Id : c.Participant1Id })
            .FirstOrDefaultAsync();

        if (chat is null)
            return AppResult<MessageResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Chat not found."]);

        // Create and save the message
        var message = mapper.Map<SendMessageRequest, Message>(request, o =>
        {
            o.Items["SenderId"] = senderId;
            o.Items["RecipientId"] = chat.ParticipantId;
        });
        appDbContext.Messages.Add(message);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Message, MessageResponse>(message);
        return AppResult<MessageResponse>.Success(response);
    }

    public async Task<IEnumerable<int>> ChangeUserStatus(int userId, bool isOnline)
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

        return chatIdsToNotify;
    }

    public async Task<bool> HasUnreadMessages(int userId)
    {
        var hasUnreadMessages = await appDbContext.Messages
            .AnyAsync(m => m.RecipientId == userId && m.IsRead == false);

        return hasUnreadMessages;
    }


    private async Task MarkReceivedMessagesAsRead(int recipientId, int chatId)
    {
        await appDbContext.Messages
            .Where(m => m.ChatId == chatId && m.RecipientId == recipientId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(m => m.IsRead, true));
    }

    private async Task<ChatSummeryResponse?> GetChatIfExist(int participant1Id, int participant2Id)
    {
        return await appDbContext.Chats
            .Where(c => (c.Participant1Id == participant1Id && c.Participant2Id == participant2Id)
                || (c.Participant1Id == participant2Id && c.Participant2Id == participant1Id))
            .ProjectTo<ChatSummeryResponse>(mapper.ConfigurationProvider)
            .AsNoTracking()
            .SingleOrDefaultAsync();
    }

    private async Task<AppResult<ChatSummeryResponse>> CreateChat(int callerId, CreateChatRequest request)
    {
        var participant = await appDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == request.ParticipantId);

        if (participant is null)
            return AppResult<ChatSummeryResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Participant not found."]);

        var chat = mapper.Map<CreateChatRequest, Chat>(request, o =>
        {
            o.Items["Participant1Id"] = callerId;
            o.Items["Participant2"] = participant;
        });

        appDbContext.Chats.Add(chat);
        await appDbContext.SaveChangesAsync();

        var response = mapper.Map<Chat, ChatSummeryResponse>(chat, o => o.Items["UserId"] = callerId);
        return AppResult<ChatSummeryResponse>.Success(response);
    }
}