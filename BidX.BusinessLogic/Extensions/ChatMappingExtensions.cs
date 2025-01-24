using BidX.BusinessLogic.DTOs.ChatDTOs;
using BidX.DataAccess.Entites;

namespace BidX.BusinessLogic.Mappings;

public static class ChatMappingExtensions
{
    public static Chat ToChatEntity(this CreateChatRequest request, int creatorId)
    {
        return new Chat
        {
            Participant1Id = creatorId,
            Participant2Id = request.ParticipantId,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public static ChatSummeryResponse ToChatSummeryResponse(this Chat chat, int currentUserId, User participant2)
    {
        var participant = chat.Participant1Id == currentUserId 
            ? participant2
            : chat.Participant1;

        return new ChatSummeryResponse
        {
            Id = chat.Id,
            ParticipantId = participant!.Id,
            ParticipantName = participant.FullName,
            ParticipantProfilePictureUrl = participant.ProfilePictureUrl,
            IsParticipantOnline = participant.IsOnline
        };
    }

    public static IQueryable<ChatDetailsResponse> ProjectToChatDetailsResponse(this IQueryable<Chat> query, int currentUserId)
    {
        return query.Select(c => new ChatDetailsResponse
        {
            Id = c.Id,
            ParticipantId = c.Participant1Id == currentUserId ? c.Participant2Id : c.Participant1Id,
            ParticipantName = c.Participant1Id == currentUserId 
                ? c.Participant2!.FullName
                : c.Participant1!.FullName,
            ParticipantProfilePictureUrl = c.Participant1Id == currentUserId 
                ? c.Participant2!.ProfilePictureUrl 
                : c.Participant1!.ProfilePictureUrl,
            IsParticipantOnline = c.Participant1Id == currentUserId 
                ? c.Participant2!.IsOnline 
                : c.Participant1!.IsOnline,
            LastMessage = c.LastMessage!.Content,
            UnreadMessagesCount = c.Messages!.Count(m => !m.IsRead && m.RecipientId == currentUserId)
        });
    }


    public static IQueryable<ChatSummeryResponse> ProjectToChatSummaryResponse(this IQueryable<Chat> query, int currentUserId)
    {
        return query.Select(c => new ChatSummeryResponse
        {
            Id = c.Id,
            ParticipantId = c.Participant1Id == currentUserId ? c.Participant2Id : c.Participant1Id,
            ParticipantName = c.Participant1Id == currentUserId 
                ? c.Participant2!.FullName
                : c.Participant1!.FullName,
            ParticipantProfilePictureUrl = c.Participant1Id == currentUserId 
                ? c.Participant2!.ProfilePictureUrl 
                : c.Participant1!.ProfilePictureUrl,
            IsParticipantOnline = c.Participant1Id == currentUserId 
                ? c.Participant2!.IsOnline 
                : c.Participant1!.IsOnline,
        });
    }
}
