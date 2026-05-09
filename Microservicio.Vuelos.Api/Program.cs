using Microservicio.Vuelos.Api.Extensions;
using Microservicio.Vuelos.Api.Middleware;

/// <summary>
/// CAMBIOS MICROSERVICIO:
///   - Eliminado: using Microservicio.Vuelos.Api.Security
///   - Eliminado: builder.Services.AddSingleton&lt;ITokenBlacklistService, TokenBlacklistService&gt;()
///     TokenBlacklistService es EXCLUSIVO del MS Seguridad.
///     Este MS solo valida la firma del JWT — no gestiona logout ni blacklist.
/// </summary>
var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();

// Controllers
builder.Services.AddControllers();

// Versioning
builder.Services.AddApiVersioningDocumentation();

// JWT — solo validación de firma, sin blacklist
builder.Services.AddJwtAuthentication(builder.Configuration);

// CORS
builder.Services.AddCorsPolicy(builder.Configuration);

// Swagger
builder.Services.AddSwaggerDocumentation();

// DbContext + Repositories + DataServices + BusinessServices + HttpClients
builder.Services.AddProjectServices(builder.Configuration);

// Authorization
builder.Services.AddAuthorization();

var app = builder.Build();

app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

// Swagger
app.UseSwaggerDocumentation();

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

// CORS
app.UseCorsPolicy();

// Authentication / Authorization
app.UseAuthentication();
app.UseAuthorization();

// Global exception handling
app.UseMiddleware<ExceptionHandlingMiddleware>();

// Controllers
app.MapControllers();

app.Run();