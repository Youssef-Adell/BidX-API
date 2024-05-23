namespace BidUp.BusinessLogic.DTOs.CommonDTOs;

public class ErrorResponse
{
    public ErrorResponse(ErrorCode errorCode, IEnumerable<string> errorMessages)
    {
        ErrorCode = errorCode;
        ErrorMessages = errorMessages;
    }

    public ErrorCode ErrorCode { get; init; }
    public IEnumerable<string> ErrorMessages { get; init; }
}