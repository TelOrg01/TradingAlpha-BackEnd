namespace TradingAlpha.Domain.Common;

/// <summary>
/// Result pattern — represents success or failure without using exceptions.
/// 
/// Why not exceptions?
/// - Wrong password is an EXPECTED outcome, not an EXCEPTIONAL one
/// - Exceptions are expensive (stack trace capture)
/// - Debugger breaks on exceptions, blocking your workflow
/// - Result forces the caller to handle both cases explicitly
/// 
/// Usage:
///   return Result<AuthResponse>.Success(response);
///   return Result<AuthResponse>.Failure("Invalid credentials.");
/// </summary>
public class Result<T>
{
    /// <summary>Whether the operation succeeded</summary>
    public bool IsSuccess { get; }

    /// <summary>The return value (only valid when IsSuccess = true)</summary>
    public T? Value { get; }

    /// <summary>Error message (only valid when IsSuccess = false)</summary>
    public string Error { get; }

    /// <summary>HTTP status code hint for the controller</summary>
    public int StatusCode { get; }

    private Result(bool isSuccess, T? value, string error, int statusCode)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        StatusCode = statusCode;
    }

    /// <summary>Create a success result with a value</summary>
    public static Result<T> Success(T value)
        => new(true, value, string.Empty, 200);

    /// <summary>Create a failure result with an error message (default 400)</summary>
    public static Result<T> Failure(string error, int statusCode = 400)
        => new(false, default, error, statusCode);

    /// <summary>Create a 404 Not Found failure</summary>
    public static Result<T> NotFound(string error)
        => new(false, default, error, 404);
}