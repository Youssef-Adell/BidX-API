namespace BidX.BusinessLogic.DTOs.CloudDTOs;

public class UploadResponse
{
    public required Guid FileId { get; set; }
    public required string FileUrl { get; set; }
}
