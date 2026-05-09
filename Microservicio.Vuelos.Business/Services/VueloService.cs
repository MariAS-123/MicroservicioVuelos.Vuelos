using System.Text.RegularExpressions;
using Microservicio.Vuelos.Business.DTOs.Vuelo;
using Microservicio.Vuelos.Business.Exceptions;
using Microservicio.Vuelos.Business.Integrations.Interfaces;
using Microservicio.Vuelos.Business.Interfaces;
using Microservicio.Vuelos.Business.Mappers;
using Microservicio.Vuelos.Business.Validators;
using Microservicio.Vuelos.DataManagement.Interfaces;
using Microservicio.Vuelos.DataManagement.Models;
using Microsoft.Extensions.Configuration;

namespace Microservicio.Vuelos.Business.Services;

/// <summary>
/// CAMBIOS MICROSERVICIO:
///
///   - IAeropuertoDataService eliminado — AeropuertoEntity no existe en BDD_Vuelos.
///     Reemplazado por IAeropuertoIntegrationService que llama al MS Aeropuertos vía HTTP.
///     La interfaz vive en Business.Integrations.Interfaces; la implementación con
///     HttpClient vive en la capa Api.
///
///   - IReservaDataService eliminado — ReservaEntity pertenece a MS Ventas.
///     GetPagedBookingAsync ya no puede filtrar por disponibilidad real de asientos
///     cruzando reservas — ese cruce lo debe hacer MS Ventas. Aquí solo filtramos
///     por estado del vuelo y fecha futura.
///
///   - DataPagedResult se construye con .Crear() en lugar de inicializador de objeto.
///
///   - Se agrega validación: no se puede crear un vuelo con fecha de salida pasada.
/// </summary>
public class VueloService : IVueloService
{
    private const int FilasCabinaEstandar = 28;
    private static readonly char[] ColumnasCabinaEstandar = ['A', 'B', 'C', 'D', 'E', 'F'];
    private const int CapacidadCabinaEstandar = FilasCabinaEstandar * 6;
    private const int UltimaFilaPrimeraClase = 4;
    private const int UltimaFilaEjecutiva = 10;
    private const decimal PrecioExtraPrimeraClase = 80m;
    private const decimal PrecioExtraEjecutiva = 40m;
    private const decimal PrecioExtraEconomica = 0m;

    private readonly IVueloDataService _vueloDataService;
    private readonly IAsientoDataService _asientoDataService;

    // CAMBIO: IAeropuertoIntegrationService en lugar de IAeropuertoDataService
    private readonly IAeropuertoIntegrationService _aeropuertoIntegration;
    private readonly IUnitOfWork _unitOfWork;
    private readonly VueloValidator _validator;
    private readonly bool _validarHoraSalidaAntesDeEnVuelo;

    public VueloService(
        IVueloDataService vueloDataService,
        IAsientoDataService asientoDataService,
        IAeropuertoIntegrationService aeropuertoIntegration,
        IUnitOfWork unitOfWork,
        IConfiguration configuration)
    {
        _vueloDataService = vueloDataService;
        _asientoDataService = asientoDataService;
        _aeropuertoIntegration = aeropuertoIntegration;
        _unitOfWork = unitOfWork;
        _validator = new VueloValidator();
        _validarHoraSalidaAntesDeEnVuelo = configuration.GetValue(
            "BusinessRules:Vuelo:ValidarHoraSalidaAntesDeEnVuelo",
            true);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET PAGED (admin)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<DataPagedResult<VueloResponseDto>> GetPagedAsync(VueloFilterDto filter)
    {
        _validator.ValidateFilter(filter);

        var filtro = VueloBusinessMapper.ToFiltroDataModel(filter);
        var result = await _vueloDataService.GetPagedAsync(filtro);

        var items = VueloBusinessMapper.ToResponseDtoList(result.Items);

        // CAMBIO: .Crear() en lugar de inicializador de objeto
        return DataPagedResult<VueloResponseDto>.Crear(items, result.TotalRegistros, result.PaginaActual, result.TamanoPagina);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET BY ID
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<VueloResponseDto?> GetByIdAsync(int idVuelo)
    {
        if (idVuelo <= 0)
            throw new ValidationException("El id del vuelo debe ser mayor que 0.");

        var data = await _vueloDataService.GetByIdAsync(idVuelo);
        return data == null ? null : VueloBusinessMapper.ToResponseDto(data);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CREATE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<VueloResponseDto> CreateAsync(VueloRequestDto request, string creadoPorUsuario)
    {
        if (string.IsNullOrWhiteSpace(creadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario creador.");

        // Backend calcula capacidad y llegada
        request.CapacidadTotal = CapacidadCabinaEstandar;
        request.FechaHoraLlegada = request.FechaHoraSalida.AddMinutes(request.DuracionMin);

        _validator.ValidateRequest(request);

        // REGLA DE NEGOCIO: no se puede crear un vuelo con fecha de salida en el pasado
        if (request.FechaHoraSalida <= DateTime.UtcNow)
            throw new BusinessException("La fecha y hora de salida no puede ser en el pasado.");

        // CAMBIO: validar aeropuertos via MS Aeropuertos (HTTP) en lugar de BDD local
        var aeropuertoOrigen = await _aeropuertoIntegration.GetAeropuertoAsync(request.IdAeropuertoOrigen);
        if (aeropuertoOrigen == null)
            throw new NotFoundException($"El aeropuerto de origen con id {request.IdAeropuertoOrigen} no existe en el sistema.");

        if (aeropuertoOrigen.Estado != "ACTIVO")
            throw new BusinessException("El aeropuerto de origen está inactivo.");

        var aeropuertoDestino = await _aeropuertoIntegration.GetAeropuertoAsync(request.IdAeropuertoDestino);
        if (aeropuertoDestino == null)
            throw new NotFoundException($"El aeropuerto de destino con id {request.IdAeropuertoDestino} no existe en el sistema.");

        if (aeropuertoDestino.Estado != "ACTIVO")
            throw new BusinessException("El aeropuerto de destino está inactivo.");

        // Verificar número de vuelo duplicado
        var existe = await _vueloDataService.GetPagedAsync(new VueloFiltroDataModel
        {
            PageNumber = 1,
            PageSize = 1,
            IncluirEliminados = false
        });
        var numeroVuelo = await GenerarNumeroVueloAsync();

        return await _unitOfWork.ExecuteInTransactionAsync(async () =>
        {
            var dataModel = VueloBusinessMapper.ToDataModel(request, creadoPorUsuario);
            dataModel.NumeroVuelo = numeroVuelo;
            dataModel.CapacidadTotal = CapacidadCabinaEstandar;

            var creado = await _vueloDataService.CreateAsync(dataModel);
            await EnsureSeatMapAsync(creado.IdVuelo, creadoPorUsuario);

            return VueloBusinessMapper.ToResponseDto(creado);
        });
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPDATE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<VueloResponseDto?> UpdateAsync(int idVuelo, VueloUpdateRequestDto request, string modificadoPorUsuario)
    {
        if (idVuelo <= 0)
            throw new ValidationException("El id del vuelo debe ser mayor que 0.");

        if (string.IsNullOrWhiteSpace(modificadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario modificador.");

        request.CapacidadTotal = CapacidadCabinaEstandar;
        request.FechaHoraLlegada = request.FechaHoraSalida.AddMinutes(request.DuracionMin);
        _validator.ValidateUpdate(request);

        var actual = await _vueloDataService.GetByIdAsync(idVuelo);
        if (actual == null)
            throw new NotFoundException("Vuelo no encontrado.");

        // No se puede modificar un vuelo en estado final
        var estadoActualCheck = actual.EstadoVuelo.Trim().ToUpperInvariant();
        if (estadoActualCheck is "ATERRIZADO" or "CANCELADO")
            throw new BusinessException($"No se puede modificar un vuelo en estado '{estadoActualCheck}'.");

        // CAMBIO: validar aeropuertos via MS Aeropuertos (HTTP)
        var aeropuertoOrigen = await _aeropuertoIntegration.GetAeropuertoAsync(request.IdAeropuertoOrigen);
        if (aeropuertoOrigen == null)
            throw new NotFoundException($"El aeropuerto de origen con id {request.IdAeropuertoOrigen} no existe en el sistema.");
        if (aeropuertoOrigen.Estado != "ACTIVO")
            throw new BusinessException("El aeropuerto de origen está inactivo.");

        var aeropuertoDestino = await _aeropuertoIntegration.GetAeropuertoAsync(request.IdAeropuertoDestino);
        if (aeropuertoDestino == null)
            throw new NotFoundException($"El aeropuerto de destino con id {request.IdAeropuertoDestino} no existe en el sistema.");
        if (aeropuertoDestino.Estado != "ACTIVO")
            throw new BusinessException("El aeropuerto de destino está inactivo.");

        // Verificar número de vuelo duplicado
        var existentes = await _vueloDataService.GetPagedAsync(new VueloFiltroDataModel
        {
            PageNumber = 1,
            PageSize = 10000
        });

        var numeroVuelo = request.NumeroVuelo.Trim().ToUpperInvariant();
        if (existentes.Items.Any(x => x.IdVuelo != idVuelo && x.NumeroVuelo.Trim().ToUpperInvariant() == numeroVuelo))
            throw new BusinessException("Ya existe otro vuelo con el mismo número de vuelo.");

        // Validar transiciones de estado
        var estadoActual = actual.EstadoVuelo.Trim().ToUpperInvariant();
        var estadoNuevo = request.EstadoVuelo.Trim().ToUpperInvariant();

        var transicionesPermitidas = new Dictionary<string, string[]>
        {
            { "PROGRAMADO", ["EN_VUELO", "CANCELADO", "DEMORADO"] },
            { "DEMORADO",   ["EN_VUELO", "CANCELADO", "PROGRAMADO"] },
            { "EN_VUELO",   ["ATERRIZADO"] },
            { "ATERRIZADO", [] },
            { "CANCELADO",  [] }
        };

        if (estadoActual != estadoNuevo)
        {
            if (!transicionesPermitidas.TryGetValue(estadoActual, out var permitidos))
                throw new ValidationException($"Estado actual del vuelo desconocido: {estadoActual}.");

            if (!permitidos.Contains(estadoNuevo))
                throw new BusinessException($"No es posible cambiar el estado de '{estadoActual}' a '{estadoNuevo}'.");
        }

        var dataModel = VueloBusinessMapper.ToDataModel(idVuelo, request);
        dataModel.ModificadoPorUsuario = modificadoPorUsuario;
        dataModel.CapacidadTotal = CapacidadCabinaEstandar;

        var actualizado = await _vueloDataService.UpdateAsync(dataModel);
        if (actualizado != null)
            await EnsureSeatMapAsync(actualizado.IdVuelo, modificadoPorUsuario);

        return actualizado == null ? null : VueloBusinessMapper.ToResponseDto(actualizado);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // UPDATE ESTADO (PATCH)
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<VueloResponseDto?> UpdateEstadoAsync(int idVuelo, VueloEstadoRequestDto request, string modificadoPorUsuario)
    {
        if (idVuelo <= 0)
            throw new ValidationException("El id del vuelo debe ser mayor que 0.");

        if (string.IsNullOrWhiteSpace(modificadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario modificador.");

        if (string.IsNullOrWhiteSpace(request.EstadoVuelo))
            throw new ValidationException("El estado del vuelo es requerido.");

        var estadosValidos = new[] { "PROGRAMADO", "EN_VUELO", "ATERRIZADO", "CANCELADO", "DEMORADO" };
        var nuevoEstado = request.EstadoVuelo.Trim().ToUpperInvariant();

        if (!estadosValidos.Contains(nuevoEstado))
            throw new ValidationException($"Estado inválido. Los valores permitidos son: {string.Join(", ", estadosValidos)}.");

        var actual = await _vueloDataService.GetByIdAsync(idVuelo);
        if (actual == null)
            throw new NotFoundException("Vuelo no encontrado.");

        var transicionesPermitidas = new Dictionary<string, string[]>
        {
            { "PROGRAMADO", ["EN_VUELO", "CANCELADO", "DEMORADO"] },
            { "DEMORADO",   ["EN_VUELO", "CANCELADO", "PROGRAMADO"] },
            { "EN_VUELO",   ["ATERRIZADO"] },
            { "ATERRIZADO", [] },
            { "CANCELADO",  [] }
        };

        var estadoActual = actual.EstadoVuelo.Trim().ToUpperInvariant();

        if (!transicionesPermitidas.TryGetValue(estadoActual, out var permitidos))
            throw new ValidationException($"Estado actual del vuelo desconocido: {estadoActual}.");

        if (!permitidos.Contains(nuevoEstado))
        {
            if (permitidos.Length == 0)
                throw new BusinessException(
                    $"El vuelo en estado '{estadoActual}' es un estado final y no puede cambiar.");

            throw new BusinessException(
                $"No es posible cambiar el estado de '{estadoActual}' a '{nuevoEstado}'. " +
                $"Transiciones permitidas: {string.Join(", ", permitidos)}.");
        }

        // REGLA: no se puede marcar EN_VUELO antes de la hora de salida
        if (_validarHoraSalidaAntesDeEnVuelo &&
            nuevoEstado == "EN_VUELO" &&
            DateTime.UtcNow < actual.FechaHoraSalida)
        {
            throw new BusinessException("No se puede marcar EN_VUELO antes de la fecha y hora de salida.");
        }

        actual.EstadoVuelo = nuevoEstado;
        actual.ModificadoPorUsuario = modificadoPorUsuario;
        actual.FechaModificacionUtc = DateTime.UtcNow;

        var actualizado = await _vueloDataService.UpdateAsync(actual);
        return actualizado == null ? null : VueloBusinessMapper.ToResponseDto(actualizado);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // DELETE
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<bool> DeleteAsync(int idVuelo, string modificadoPorUsuario)
    {
        if (idVuelo <= 0)
            throw new ValidationException("El id del vuelo debe ser mayor que 0.");

        if (string.IsNullOrWhiteSpace(modificadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario modificador.");

        var actual = await _vueloDataService.GetByIdAsync(idVuelo);
        if (actual == null)
            throw new NotFoundException("Vuelo no encontrado.");

        // No se puede eliminar un vuelo EN_VUELO
        if (actual.EstadoVuelo.Trim().ToUpperInvariant() == "EN_VUELO")
            throw new BusinessException("No se puede eliminar un vuelo que está en vuelo.");

        return await _vueloDataService.DeleteAsync(idVuelo, modificadoPorUsuario);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // GET PAGED BOOKING (portal cliente)
    // CAMBIO: se eliminó el cruce con reservas — ReservaDataService no existe
    // en MS Vuelos. Solo filtramos por estado del vuelo y fecha futura.
    // MS Ventas es responsable de cruzar disponibilidad real de asientos.
    // ─────────────────────────────────────────────────────────────────────────

    public async Task<DataPagedResult<VueloResponseDto>> GetPagedBookingAsync(VueloFilterDto filter)
    {
        _validator.ValidateFilterBooking(filter);

        var filtroCompleto = VueloBusinessMapper.ToFiltroDataModel(filter);
        filtroCompleto.PageNumber = 1;
        filtroCompleto.PageSize = 100000;

        var resultCompleto = await _vueloDataService.GetPagedAsync(filtroCompleto);

        var vuelosFiltrados = resultCompleto.Items
            .Where(v =>
                string.Equals(v.Estado, "ACTIVO", StringComparison.OrdinalIgnoreCase) &&
                (v.EstadoVuelo == "PROGRAMADO" || v.EstadoVuelo == "DEMORADO") &&
                v.FechaHoraSalida > DateTime.UtcNow)
            .ToList();

        var page = filter.Page <= 0 ? 1 : filter.Page;
        var pageSize = filter.PageSize <= 0 ? 20 : filter.PageSize;
        var total = vuelosFiltrados.Count;

        var items = vuelosFiltrados
            .OrderBy(v => v.FechaHoraSalida)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return DataPagedResult<VueloResponseDto>.Crear(
            VueloBusinessMapper.ToResponseDtoList(items),
            total,
            page,
            pageSize);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // HELPERS PRIVADOS
    // ─────────────────────────────────────────────────────────────────────────

    private async Task<string> GenerarNumeroVueloAsync()
    {
        var existentes = await _vueloDataService.GetPagedAsync(new VueloFiltroDataModel
        {
            PageNumber = 1,
            PageSize = 100000
        });

        var correlativoMaximo = 0;
        var regex = new Regex("^AV(\\d+)$", RegexOptions.Compiled | RegexOptions.CultureInvariant);

        foreach (var vuelo in existentes.Items)
        {
            var numeroVuelo = (vuelo.NumeroVuelo ?? string.Empty).Trim().ToUpperInvariant();
            var match = regex.Match(numeroVuelo);
            if (!match.Success) continue;

            if (int.TryParse(match.Groups[1].Value, out var valor) && valor > correlativoMaximo)
                correlativoMaximo = valor;
        }

        return $"AV{(correlativoMaximo + 1):D4}";
    }

    private async Task EnsureSeatMapAsync(int idVuelo, string usuario)
    {
        var existentes = await _asientoDataService.GetByVueloAsync(idVuelo);
        var numerosExistentes = existentes
            .Where(x => !x.Eliminado)
            .Select(x => x.NumeroAsiento.Trim().ToUpperInvariant())
            .ToHashSet();

        foreach (var fila in Enumerable.Range(1, FilasCabinaEstandar))
        {
            foreach (var columna in ColumnasCabinaEstandar)
            {
                var numeroAsiento = $"{fila}{columna}";
                if (numerosExistentes.Contains(numeroAsiento))
                    continue;

                var posicion = columna switch
                {
                    'A' or 'F' => "VENTANA",
                    'B' or 'E' => "CENTRO",
                    _ => "PASILLO"
                };

                var (clase, precioExtra) = ObtenerConfiguracionAsiento(fila);

                await _asientoDataService.CreateAsync(new AsientoDataModel
                {
                    IdVuelo = idVuelo,
                    NumeroAsiento = numeroAsiento,
                    Clase = clase,
                    Disponible = true,
                    PrecioExtra = precioExtra,
                    Posicion = posicion,
                    Estado = "ACTIVO",
                    Eliminado = false,
                    CreadoPorUsuario = usuario
                });
            }
        }
    }

    private static (string Clase, decimal PrecioExtra) ObtenerConfiguracionAsiento(int fila)
    {
        if (fila <= UltimaFilaPrimeraClase)
            return ("PRIMERA", PrecioExtraPrimeraClase);

        if (fila <= UltimaFilaEjecutiva)
            return ("EJECUTIVA", PrecioExtraEjecutiva);

        return ("ECONOMICA", PrecioExtraEconomica);
    }
}