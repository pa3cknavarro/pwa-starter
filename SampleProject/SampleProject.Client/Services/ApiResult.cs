using SampleProject.Client.Models;

namespace SampleProject.Client.Services;

/// <summary>
/// Result wrapper returned by the client for every HTTP call.
/// </summary>
public class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public int StatusCode { get; init; }
    public T? Data { get; init; }
    public ErrorEnvelope? Error { get; init; }

    public static ApiResult<T> Success(int status, T data) =>
        new() { IsSuccess = true, StatusCode = status, Data = data };

    public static ApiResult<T> Failure(int status, ErrorEnvelope? error) =>
        new() { IsSuccess = false, StatusCode = status, Error = error };
}
