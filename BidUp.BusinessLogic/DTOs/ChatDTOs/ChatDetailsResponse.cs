namespace BidUp.BusinessLogic.DTOs.ChatDTOs;

public class ChatDetailsResponse
{
    public int Id { get; init; }
    public int ParticipantId { get; init; }
    public required string ParticipantName { get; init; }
    public string? ParticipantProfilePictureUrl { get; init; }
    public required string LastMessage { get; init; }
    public bool HasUnseenMessages { get; init; }
}
