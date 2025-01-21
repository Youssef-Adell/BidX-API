namespace BidUp.BusinessLogic.DTOs.ChatDTOs;

public class ChatDetailsResponse
{
    public int Id { get; init; }
    public int ParticipantId { get; init; }
    public required string ParticipantName { get; init; }
    public string? ParticipantProfilePictureUrl { get; init; }
    public bool IsParticipantOnline { get; init; }
    public string? LastMessage { get; init; }
    public int UnreadMessagesCount { get; init; }
}
