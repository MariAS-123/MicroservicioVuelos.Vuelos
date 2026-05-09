namespace Microservicio.Vuelos.Business.Exceptions;

public class ValidationException : BusinessException
{
    public IReadOnlyCollection<string> Errors { get; }

    public ValidationException(string message)
        : base("VALIDATION_ERROR", message, 422)
    {
        Errors = Array.Empty<string>();
    }

    public ValidationException(string message, IEnumerable<string> errors)
        : base("VALIDATION_ERROR", message, 422)
    {
        Errors = errors?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray()
            ?? Array.Empty<string>();
    }
}