namespace BidUp.BusinessLogic.DTOs.CommonDTOs;

public class ErrorResponse
{
    public ErrorResponse(ErrorCode errorCode, string errorMessage)
    {
        ErrorCode = errorCode.ToString();
        ErrorMessage = errorMessage;
    }

    public string ErrorCode { get; init; }
    public string ErrorMessage { get; init; }
}