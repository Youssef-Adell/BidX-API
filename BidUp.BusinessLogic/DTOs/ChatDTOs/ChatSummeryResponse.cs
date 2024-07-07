namespace BidUp.BusinessLogic.DTOs.ChatDTOs;

public class ChatSummeryResponse
{
    public int Id { get; init; }
    public int ParticipantId { get; init; }
    public required string ParticipantName { get; init; }
    public string? ParticipantProfilePicture { get; init; }
}
