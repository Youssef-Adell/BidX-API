using AutoMapper;
using BidUp.BusinessLogic.DTOs.ChatDTOs;
using BidUp.BusinessLogic.DTOs.CommonDTOs;
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
            ParticipantProfilePicture = receiver.ProfilePictureUrl,
        };

        return AppResult<ChatSummeryResponse>.Success(response);
    }

    public async Task<AppResult<MessageResponse>> SendMessage(int senderId, MessageRequest messageRequest)
    {
        var chat = await appDbContext.UserChats
            .GroupBy(uc => uc.ChatId)
            .Select(g => new Chat
            {
                Id = g.Key,
                Users = g.Select(uc => uc.User).ToList()!
            })
            .FirstOrDefaultAsync(g => g.Id == messageRequest.ChatId);

        if (chat is null || !chat.Users.Any(u => u.Id == senderId))
            return AppResult<MessageResponse>.Failure(ErrorCode.RESOURCE_NOT_FOUND, ["Chat not found."]);


        // Save the message to the db
        var message = new Message
        {
            ChatId = chat.Id,
            SenderId = senderId,
            Content = messageRequest.Message,
        };
        appDbContext.Messages.Add(message);
        await appDbContext.SaveChangesAsync();


        // Map the message to a response
        var response = new MessageResponse
        {
            Id = message.Id,
            Content = message.Content,
            SentAt = message.SentAt,
            SenderId = message.SenderId,
            ChatId = message.ChatId,
            ReceiverId = chat.Users.First(u => u.Id != senderId).Id,
        };
        return AppResult<MessageResponse>.Success(response);
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
