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


public class AppResult<T>
{
    /// <summary>
    /// Creates successfull result with data of type T.
    /// </summary>
    public AppResult(T data)
    {
        Data = data;
    }

    /// <summary>
    /// Creates unsuccessfull result.
    /// </summary>
    public AppResult(ErrorCode errorCode, string errorMessage)
    {
        Error = new(errorCode, errorMessage);
    }

    public T? Data { get; set; }
    public ErrorResponse? Error { get; init; }
    public bool Succeeded => Error == null;
}