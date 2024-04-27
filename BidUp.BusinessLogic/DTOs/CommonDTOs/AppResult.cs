namespace BidUp.BusinessLogic.DTOs.CommonDTOs;

/// <summary>A class used to represent the results returned from the application services.</summary>
public class AppResult
{
    private AppResult()
    {
    }

    private AppResult(ErrorCode errorCode, string errorMessage)
    {
        Error = new(errorCode, errorMessage);
    }

    public ErrorResponse? Error { get; init; }
    public bool Succeeded => Error == null;

    /// <returns>AppResult object represents an unsuccessfull result.</returns>
    public static AppResult Failure(ErrorCode errorCode, string errorMessage) => new(errorCode, errorMessage);

    /// <returns>AppResult object represents a successfull result.</returns>
    public static AppResult Success() => new();

}


/// <summary>A class used to represent the results returned from the application services.</summary>
public class AppResult<TData>
{
    private AppResult(TData data)
    {
        Data = data;
    }

    private AppResult(ErrorCode errorCode, string errorMessage)
    {
        Error = new(errorCode, errorMessage);
    }

    public TData? Data { get; set; }
    public ErrorResponse? Error { get; init; }
    public bool Succeeded => Error == null;

    /// <returns>AppResult object represents an unsuccessfull result.</returns>
    public static AppResult<TData> Failure(ErrorCode errorCode, string errorMessage) => new(errorCode, errorMessage);

    /// <returns>AppResult object represents a successfull result.</returns>
    public static AppResult<TData> Success(TData data) => new(data);
}