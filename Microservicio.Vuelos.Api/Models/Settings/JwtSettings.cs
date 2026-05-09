namespace Microservicio.Vuelos.Api.Models.Settings;

/// <summary>
/// NOTA MICROSERVICIO: ExpirationMinutes se mantiene en la clase pero NO se valida
/// en AutenticationExtensions porque este MS no emite tokens — solo valida la firma.
/// El valor puede quedar en 0 sin problema.
/// </summary>
public class JwtSettings
{
    public const string SectionName = "JwtSettings";

    public string SecretKey { get; set; } = string.Empty;

    public string Issuer { get; set; } = string.Empty;

    public string Audience { get; set; } = string.Empty;

    public int ExpirationMinutes { get; set; }
}