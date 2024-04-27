namespace BidUp.BusinessLogic.DTOs.CommonDTOs;

public class ErrorResponse
{
    public ErrorResponse(ErrorCode errorCode, string errorMessage)
    {
        ErrorCode = errorCode;
        ErrorMessage = errorMessage;
    }

    public ErrorCode ErrorCode { get; init; }
    public string ErrorMessage { get; init; }
}