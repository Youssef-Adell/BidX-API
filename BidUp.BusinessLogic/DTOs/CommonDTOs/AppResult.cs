namespace BidUp.BusinessLogic.DTOs.CommonDTOs;

/// <summary>A class used to represent the results returned from the application services.</summary>
public class AppResult
{
    private AppResult()
    {
    }

    private AppResult(ErrorCode errorCode, IEnumerable<string> errorMessages)
    {
        Error = new(errorCode, errorMessages);
    }

    public ErrorResponse? Error { get; init; }
    public bool Succeeded => Error == null;

    /// <returns>AppResult object represents an unsuccessfull result.</returns>
    public static AppResult Failure(ErrorCode errorCode, IEnumerable<string> errorMessages) => new(errorCode, errorMessages);

    /// <returns>AppResult object represents a successfull result.</returns>
    public static AppResult Success() => new();

}


/// <summary>A class used to represent the results returned from the application services.</summary>
public class AppResult<TResponse>
{
    private AppResult(TResponse response)
    {
        Response = response;
    }

    private AppResult(ErrorCode errorCode, IEnumerable<string> errorMessages)
    {
        Error = new(errorCode, errorMessages);
    }

    public TResponse? Response { get; set; }
    public ErrorResponse? Error { get; init; }
    public bool Succeeded => Error == null;

    /// <returns>AppResult object represents an unsuccessfull result.</returns>
    public static AppResult<TResponse> Failure(ErrorCode errorCode, IEnumerable<string> errorMessages) => new(errorCode, errorMessages);

    /// <returns>AppResult object represents a successfull result.</returns>
    public static AppResult<TResponse> Success(TResponse response) => new(response);
}