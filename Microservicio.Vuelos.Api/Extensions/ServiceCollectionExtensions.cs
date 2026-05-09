using System.Reflection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microservicio.Vuelos.DataAccess.Context;
using Microservicio.Vuelos.Business.Integrations.Interfaces;
using Microservicio.Vuelos.Api.Integrations;

namespace Microservicio.Vuelos.Api.Extensions;

/// <summary>
/// CAMBIOS MICROSERVICIO:
///   - ConnectionStringName cambia a "MicroservicioVuelosDb" → apunta a BDD_Vuelos.
///   - SistemaVuelosDBContext → VuelosDbContext.
///   - Se agrega registro de IAeropuertoIntegrationService con su HttpClient.
///     La URL base viene de appsettings.json: ServiciosExternos:AeropuertosBaseUrl.
///   - RegisterServicesByConvention para Business excluye las interfaces de
///     Integrations (IAeropuertoIntegrationService) porque su implementación
///     vive en la capa Api, no en Business.Services — se registra manualmente.
/// </summary>
public static class ServiceCollectionExtensions
{
    private const string ConnectionStringName = "MicroservicioVuelosDb";

    public static IServiceCollection AddProjectServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        RegisterDbContext(services, configuration);
        RegisterRepositoriesByConvention(services, "Microservicio.Vuelos.DataAccess");
        RegisterUnitOfWork(services, "Microservicio.Vuelos.DataManagement");
        RegisterServicesByConvention(services, "Microservicio.Vuelos.DataManagement");
        RegisterServicesByConvention(services, "Microservicio.Vuelos.Business");

        // NUEVO: registrar HttpClient y servicio de integración con MS Aeropuertos
        RegisterAeropuertoIntegration(services, configuration);

        return services;
    }

    private static void RegisterDbContext(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString(ConnectionStringName);

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"La cadena de conexión '{ConnectionStringName}' no está configurada.");

        // CAMBIO: VuelosDbContext en lugar de SistemaVuelosDBContext
        services.AddDbContext<VuelosDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.EnableRetryOnFailure(
                    maxRetryCount: 5,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            });
        });
    }

    // NUEVO: registra el HttpClient con la URL del MS Aeropuertos
    // y la implementación de IAeropuertoIntegrationService
    private static void RegisterAeropuertoIntegration(
        IServiceCollection services,
        IConfiguration configuration)
    {
        var aeropuertosBaseUrl = configuration["ServiciosExternos:AeropuertosBaseUrl"];

        if (string.IsNullOrWhiteSpace(aeropuertosBaseUrl))
            throw new InvalidOperationException(
                "ServiciosExternos:AeropuertosBaseUrl no está configurada en appsettings.json.");

        services.AddHttpClient("aeropuertos", client =>
        {
            client.BaseAddress = new Uri(aeropuertosBaseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        services.AddScoped<IAeropuertoIntegrationService, AeropuertoIntegrationService>();
    }

    private static void RegisterRepositoriesByConvention(
        IServiceCollection services,
        string assemblyName)
    {
        var assembly = LoadAssembly(assemblyName);

        var interfaces = assembly.GetTypes()
            .Where(t =>
                t.IsInterface &&
                t.IsPublic &&
                t.Namespace is not null &&
                t.Namespace.Contains(".Repositories.Interfaces", StringComparison.Ordinal))
            .ToList();

        var implementations = assembly.GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsPublic &&
                t.Namespace is not null &&
                t.Namespace.Contains(".Repositories", StringComparison.Ordinal) &&
                !t.Namespace.Contains(".Interfaces", StringComparison.Ordinal))
            .ToList();

        RegisterByConvention(services, interfaces, implementations);
    }

    private static void RegisterUnitOfWork(
        IServiceCollection services,
        string assemblyName)
    {
        var assembly = LoadAssembly(assemblyName);

        var interfaceType = assembly.GetTypes()
            .FirstOrDefault(t =>
                t.IsInterface &&
                t.Name.Equals("IUnitOfWork", StringComparison.Ordinal));

        var implementationType = assembly.GetTypes()
            .FirstOrDefault(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.Name.Equals("UnitOfWork", StringComparison.Ordinal));

        if (interfaceType is null)
            throw new InvalidOperationException(
                $"No se encontró IUnitOfWork en '{assemblyName}'.");

        if (implementationType is null)
            throw new InvalidOperationException(
                $"No se encontró UnitOfWork en '{assemblyName}'.");

        services.TryAddScoped(interfaceType, implementationType);
    }

    private static void RegisterServicesByConvention(
        IServiceCollection services,
        string assemblyName)
    {
        var assembly = LoadAssembly(assemblyName);

        var interfaces = assembly.GetTypes()
            .Where(t =>
                t.IsInterface &&
                t.IsPublic &&
                t.Namespace is not null &&
                t.Namespace.Contains(".Interfaces", StringComparison.Ordinal) &&
                !t.Name.Equals("IUnitOfWork", StringComparison.Ordinal) &&
                // NUEVO: excluir interfaces de Integrations — se registran manualmente
                !t.Namespace.Contains(".Integrations", StringComparison.Ordinal))
            .ToList();

        var implementations = assembly.GetTypes()
            .Where(t =>
                t.IsClass &&
                !t.IsAbstract &&
                t.IsPublic &&
                t.Namespace is not null &&
                t.Namespace.Contains(".Services", StringComparison.Ordinal))
            .ToList();

        RegisterByConvention(services, interfaces, implementations);
    }

    private static void RegisterByConvention(
        IServiceCollection services,
        List<Type> interfaces,
        List<Type> implementations)
    {
        foreach (var interfaceType in interfaces)
        {
            var expectedImplementationName = interfaceType.Name.StartsWith("I", StringComparison.Ordinal)
                ? interfaceType.Name[1..]
                : interfaceType.Name;

            var implementationType = implementations.FirstOrDefault(t =>
                t.Name.Equals(expectedImplementationName, StringComparison.Ordinal));

            if (implementationType is null)
                continue;

            services.TryAddScoped(interfaceType, implementationType);
        }
    }

    private static Assembly LoadAssembly(string assemblyName)
    {
        try
        {
            return Assembly.Load(assemblyName);
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException(
                $"No se pudo cargar el ensamblado '{assemblyName}'.", ex);
        }
    }
}