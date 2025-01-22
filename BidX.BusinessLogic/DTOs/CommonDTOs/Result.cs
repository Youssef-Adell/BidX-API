namespace BidX.BusinessLogic.DTOs.CommonDTOs;

/// <summary>
/// Represents the result of an application operation that doesn't return a value.
/// Provides a consistent way to handle both successful and failed operations.
/// </summary>
public class Result
{
    // Private constructor ensures factory methods are used
    private Result() { }

    /// <summary>
    /// Gets the error details if the operation failed.
    /// </summary>
    public ErrorResponse? Error { get; private init; }

    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Succeeded => Error is null;

    /// <summary>
    /// Creates a successful result.
    /// </summary>
    public static Result Success() => new();

    /// <summary>
    /// Creates a failed result with the specified error code and messages.
    /// </summary>
    public static Result Failure(ErrorCode errorCode, IEnumerable<string> errorMessages) =>
        new() { Error = new ErrorResponse(errorCode, errorMessages) };

    /// <summary>
    /// Creates a failed result with the specified error response.
    /// </summary>
    public static Result Failure(ErrorResponse error) =>
        new() { Error = error };
}

/// <summary>
/// Represents the result of an application operation that returns a value.
/// Provides a consistent way to handle both successful and failed operations.
/// </summary>
/// <typeparam name="TResponse">The type of the response value.</typeparam>
public class Result<TResponse>
{
    // Private constructor ensures factory methods are used
    private Result() { }

    /// <summary>
    /// Gets the response value if the operation was successful.
    /// </summary>
    public TResponse? Response { get; private init; }

    /// <summary>
    /// Gets the error details if the operation failed.
    /// </summary>
    public ErrorResponse? Error { get; private init; }

    /// <summary>
    /// Indicates whether the operation was successful.
    /// </summary>
    public bool Succeeded => Error is null;

    /// <summary>
    /// Creates a successful result with the specified response value.
    /// </summary>
    public static Result<TResponse> Success(TResponse response) =>
        new() { Response = response };

    /// <summary>
    /// Creates a failed result with the specified error code and messages.
    /// </summary>
    public static Result<TResponse> Failure(ErrorCode errorCode, IEnumerable<string> errorMessages) =>
        new() { Error = new ErrorResponse(errorCode, errorMessages) };

    /// <summary>
    /// Creates a failed result with the specified error response.
    /// </summary>
    public static Result<TResponse> Failure(ErrorResponse error) =>
        new() { Error = error };
}