using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Mappings;

public static class MessageMappingExtensions
{
    public static Message ToMessageEntity(this SendMessageRequest request, int senderId, int recipientId)
    {
        return new Message
        {
            Content = request.Message,
            ChatId = request.ChatId,
            SentAt = DateTimeOffset.UtcNow,
            IsRead = false,
            SenderId = senderId,
            RecipientId = recipientId
        };
    }

    public static MessageResponse ToMessageResponse(this Message message)
    {
        return new MessageResponse
        {
            Id = message.Id,
            Content = message.Content,
            SentAt = message.SentAt,
            IsRead = message.IsRead,
            ChatId = message.ChatId,
            SenderId = message.SenderId,
            RecipientId = message.RecipientId
        };
    }

    public static IQueryable<MessageResponse> ProjectToMessageResponse(this IQueryable<Message> query)
    {
        return query.Select(m => new MessageResponse
        {
            Id = m.Id,
            Content = m.Content,
            SentAt = m.SentAt,
            IsRead = m.IsRead,
            ChatId = m.ChatId,
            SenderId = m.SenderId,
            RecipientId = m.RecipientId
        });
    }
}