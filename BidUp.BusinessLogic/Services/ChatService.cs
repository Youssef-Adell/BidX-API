using AutoMapper;
using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
using BidUp.BusinessLogic.DTOs.QueryParamsDTOs;
using BidUp.BusinessLogic.Interfaces;
using BidUp.DataAccess;
using BidUp.DataAccess.Entites;
using Microsoft.EntityFrameworkCore;

namespace BidUp.BusinessLogic.Services;

public class ChatService : IChatService
{
    private readonly AppDbContext appDbContext;
    private readonly IMapper mapper;

    public ChatService(AppDbContext appDbContext, IMapper mapper)
    {
        this.appDbContext = appDbContext;
        this.mapper = mapper;

    }


    public async Task<Page<ChatDetailsResponse>> GetUserChats(int userId, ChatsQueryParams queryParams)
    {
        var userChatsQuery = appDbContext.Chats
            .Include(c => c.Users)
            .Include(c => c.Messages)
            .Where(c => c.Users.Any(u => u.Id == userId) && c.Messages.Count() > 0)
            .Select(c => new ChatDetailsResponse
            {
                Id = c.Id,
                ParticipantId = c.Users.First(u => u.Id != userId).Id,
                ParticipantName = $"{c.Users.First(u => u.Id != userId).FirstName}",
                ParticipantProfilePictureUrl = c.Users.First(u => u.Id != userId).ProfilePictureUrl,
                LastMessage = c.Messages.OrderBy(c => c.Id).Last().Content,
                HasUnseenMessages = c.Messages.Any(m => m.SenderId != userId && !m.Seen)
            });

        var totalCount = await userChatsQuery.CountAsync();

        var userChatsResponses = await userChatsQuery
            // Get the new chats first
            .OrderByDescending(c => c.Id)
            // Paginate
            .Skip((queryParams.Page - 1) * queryParams.PageSize)
            .Take(queryParams.PageSize)
            .AsNoTracking()
            .ToListAsync();

        var response = new Page<ChatDetailsResponse>(userChatsResponses, queryParams.Page, queryParams.PageSize, totalCount);
        return response;
    }

    public async Task<AppResult<ChatSummeryResponse>> IntiateChat(int senderId, int receiverId)
    {
        var receiver = await appDbContext.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == receiverId);

        if (receiver is null)
            return AppResult<ChatSummeryResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Receiver not found."]);


        var chat = await appDbContext.Chats
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Users.Any(u => u.Id == senderId) && c.Users.Any(u => u.Id == receiverId));

        chat ??= await CreateChat(senderId, receiverId);


        var response = new ChatSummeryResponse
        {
            Id = chat.Id,
            ParticipantId = receiver.Id,
            ParticipantName = $"{receiver.FirstName} {receiver.LastName}",
            ParticipantProfilePictureUrl = receiver.ProfilePictureUrl,
            IsParticipantOnline = receiver.IsOnline,
        };

        return AppResult<ChatSummeryResponse>.Success(response);
    }

    public async Task<AppResult<MessageResponse>> SendMessage(int senderId, MessageRequest messageRequest)
    {
        var chatExists = await appDbContext.Chats
            .AnyAsync(c => c.Id == messageRequest.ChatId && c.Users.Any(u => u.Id == senderId));

        if (!chatExists)
            return AppResult<MessageResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Chat not found."]);


        // Save the message to the db
        var message = new Message
        {
            ChatId = messageRequest.ChatId,
            SenderId = senderId,
            Content = messageRequest.Message,
        };
        appDbContext.Messages.Add(message);
        await appDbContext.SaveChangesAsync();


        // Map the message to a response
        var response = new MessageResponse
        {
            Id = message.Id,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            Content = message.Content,
            SentAt = message.SentAt,
            Seen = message.Seen,
            ReceiverId = (await appDbContext.UserChats.FirstAsync(uc => uc.ChatId == messageRequest.ChatId && uc.UserId != senderId)).UserId,
        };
        return AppResult<MessageResponse>.Success(response);
    }

    public async Task<AppResult> MarkReceivedMessagesAsSeen(int userId, int chatId)
    {
        var chatExists = await appDbContext.Chats
            .AnyAsync(c => c.Id == chatId && c.Users.Any(u => u.Id == userId));

        if (!chatExists)
            return AppResult.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Chat not found."]);

        var unseenMessages = await appDbContext.Messages
            .Where(m => m.ChatId == chatId && m.SenderId != userId && !m.Seen)
            .ToListAsync();

        unseenMessages.ForEach(m => m.Seen = true);

        await appDbContext.SaveChangesAsync();
        return AppResult.Success();
    }

    public async Task<IEnumerable<int>> ChangeUserStatus(int userId, bool isOnline)
    {
        await appDbContext.Users
            .Where(u => u.Id == userId)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.IsOnline, isOnline));

        var chatIdsToNotify = await appDbContext.UserChats
            .Where(uc => uc.UserId == userId)
            .Select(uc => uc.ChatId)
            .ToListAsync();

        return chatIdsToNotify;
    }

    public async Task<bool> HasUnseenMessages(int userId)
    {
        var hasUnseenMessages = await appDbContext.Messages
            .AnyAsync(m => m.Chat!.Users.Any(u => u.Id == userId) && m.SenderId != userId && !m.Seen);

        if (hasUnseenMessages)
            return true;

        return false;
    }


    private async Task<Chat> CreateChat(int senderId, int receiverId)
    {
        var chat = new Chat();

        var userChatEntries = new List<UserChat>{
            new () { Chat = chat, UserId = senderId },
            new () { Chat = chat, UserId = receiverId }
        };

        appDbContext.UserChats.AddRange(userChatEntries);
        await appDbContext.SaveChangesAsync();

        return chat;
    }
}
