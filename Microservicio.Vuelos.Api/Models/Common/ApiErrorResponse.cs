namespace Microservicio.Vuelos.Api.Models.Common;

public class ApiErrorResponse
{
    public bool Success { get; set; } = false;

    public string Message { get; set; } = string.Empty;

    public IReadOnlyCollection<string> Errors { get; set; } = Array.Empty<string>();

    public string? TraceId { get; set; }

    public static ApiErrorResponse Create(
        string message,
        IReadOnlyCollection<string>? errors = null,
        string? traceId = null)
    {
        return new ApiErrorResponse
        {
            Success = false,
            Message = message,
            Errors = errors ?? Array.Empty<string>(),
            TraceId = traceId
        };
    }
}