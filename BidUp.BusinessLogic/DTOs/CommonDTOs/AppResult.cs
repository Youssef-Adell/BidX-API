namespace BidUp.BusinessLogic.DTOs.CommonDTOs;

public class AppResult
{
    /// <summary>
    /// Creates successfull result.
    /// </summary>
    public AppResult()
    {
    }

    /// <summary>
    /// Creates unsuccessfull result.
    /// </summary>
    public AppResult(ErrorCode errorCode, string errorMessage)
    {
        Error = new(errorCode, errorMessage);
    }

    public ErrorResponse? Error { get; init; }
    public bool Succeeded => Error == null;
}