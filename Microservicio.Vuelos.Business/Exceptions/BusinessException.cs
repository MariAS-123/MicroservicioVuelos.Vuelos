namespace Microservicio.Vuelos.Business.Exceptions;

public class BusinessException : Exception
{
    public string Code { get; }

    public int StatusCode { get; }

    public BusinessException(string message)
        : this("BUSINESS_ERROR", message, 400)
    {
    }

    public BusinessException(string code, string message, int statusCode)
        : base(message)
    {
        Code = code;
        StatusCode = statusCode;
    }

    public BusinessException(string code, string message, int statusCode, Exception innerException)
        : base(message, innerException)
    {
        Code = code;
        StatusCode = statusCode;
    }
}