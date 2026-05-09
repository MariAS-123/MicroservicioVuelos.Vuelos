using Microservicio.Vuelos.Business.DTOs.Escala;
using Microservicio.Vuelos.Business.Exceptions;
using Microservicio.Vuelos.Business.Integrations.Interfaces;
using Microservicio.Vuelos.Business.Interfaces;
using Microservicio.Vuelos.Business.Mappers;
using Microservicio.Vuelos.Business.Validators;
using Microservicio.Vuelos.DataManagement.Interfaces;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.Business.Services;

/// <summary>
/// CAMBIO MICROSERVICIO:
///   - IAeropuertoDataService eliminado — AeropuertoEntity no existe en BDD_Vuelos.
///     Reemplazado por IAeropuertoIntegrationService que llama al MS Aeropuertos vía HTTP.
///   - DataPagedResult se construye con .Crear() en lugar de inicializador de objeto.
/// </summary>
public class EscalaService : IEscalaService
{
    private readonly IEscalaDataService _escalaDataService;
    private readonly IVueloDataService _vueloDataService;
    private readonly IAeropuertoIntegrationService _aeropuertoIntegration;
    private readonly EscalaValidator _validator;

    public EscalaService(
        IEscalaDataService escalaDataService,
        IVueloDataService vueloDataService,
        IAeropuertoIntegrationService aeropuertoIntegration)
    {
        _escalaDataService = escalaDataService;
        _vueloDataService = vueloDataService;
        _aeropuertoIntegration = aeropuertoIntegration;
        _validator = new EscalaValidator();
    }

    public async Task<DataPagedResult<EscalaResponseDto>> GetPagedAsync(EscalaFilterDto filter)
    {
        _validator.ValidateFilter(filter);

        var filtro = EscalaBusinessMapper.ToFiltroDataModel(filter);
        var result = await _escalaDataService.GetPagedAsync(filtro);

        return DataPagedResult<EscalaResponseDto>.Crear(
            EscalaBusinessMapper.ToResponseDtoList(result.Items),
            result.TotalRegistros,
            result.PaginaActual,
            result.TamanoPagina);
    }

    public async Task<EscalaResponseDto?> GetByIdAsync(int idEscala)
    {
        if (idEscala <= 0)
            throw new ValidationException("El id de la escala debe ser mayor que 0.");

        var data = await _escalaDataService.GetByIdAsync(idEscala);
        return data == null ? null : EscalaBusinessMapper.ToResponseDto(data);
    }

    public async Task<EscalaResponseDto> CreateAsync(EscalaRequestDto request, string creadoPorUsuario)
    {
        if (string.IsNullOrWhiteSpace(creadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario creador.");

        _validator.ValidateRequest(request);

        // Validar que el vuelo existe y permite escalas
        var vuelo = await _vueloDataService.GetByIdAsync(request.IdVuelo);
        if (vuelo == null)
            throw new NotFoundException("El vuelo indicado no existe.");
        if (vuelo.Estado != "ACTIVO" || vuelo.EstadoVuelo is "CANCELADO" or "ATERRIZADO")
            throw new BusinessException("No se pueden crear escalas para un vuelo inactivo, cancelado o aterrizado.");

        // CAMBIO: validar aeropuerto via MS Aeropuertos (HTTP)
        var aeropuerto = await _aeropuertoIntegration.GetAeropuertoAsync(request.IdAeropuerto);
        if (aeropuerto == null)
            throw new NotFoundException($"El aeropuerto con id {request.IdAeropuerto} no existe en el sistema.");
        if (aeropuerto.Estado != "ACTIVO")
            throw new BusinessException("El aeropuerto indicado está inactivo.");

        // Validar orden único dentro del vuelo
        var existentes = await _escalaDataService.GetPagedAsync(new EscalaFiltroDataModel
        {
            IdVuelo = request.IdVuelo,
            PageNumber = 1,
            PageSize = 10000
        });

        if (existentes.Items.Any(x => x.IdVuelo == request.IdVuelo && x.Orden == request.Orden))
            throw new BusinessException("Ya existe una escala con el mismo orden para el vuelo indicado.");

        var dataModel = EscalaBusinessMapper.ToDataModel(request, creadoPorUsuario);
        var creada = await _escalaDataService.CreateAsync(dataModel);

        return EscalaBusinessMapper.ToResponseDto(creada);
    }

    public async Task<EscalaResponseDto?> UpdateAsync(int idEscala, EscalaUpdateRequestDto request, string modificadoPorUsuario)
    {
        if (idEscala <= 0)
            throw new ValidationException("El id de la escala debe ser mayor que 0.");

        if (string.IsNullOrWhiteSpace(modificadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario modificador.");

        _validator.ValidateUpdate(request);

        var actual = await _escalaDataService.GetByIdAsync(idEscala);
        if (actual == null)
            throw new NotFoundException("Escala no encontrada.");

        var vuelo = await _vueloDataService.GetByIdAsync(request.IdVuelo);
        if (vuelo == null)
            throw new NotFoundException("El vuelo indicado no existe.");
        if (vuelo.Estado != "ACTIVO" || vuelo.EstadoVuelo is "CANCELADO" or "ATERRIZADO")
            throw new BusinessException("No se pueden actualizar escalas para un vuelo inactivo, cancelado o aterrizado.");

        // CAMBIO: validar aeropuerto via MS Aeropuertos (HTTP)
        var aeropuerto = await _aeropuertoIntegration.GetAeropuertoAsync(request.IdAeropuerto);
        if (aeropuerto == null)
            throw new NotFoundException($"El aeropuerto con id {request.IdAeropuerto} no existe en el sistema.");
        if (aeropuerto.Estado != "ACTIVO")
            throw new BusinessException("El aeropuerto indicado está inactivo.");

        // Validar orden único dentro del vuelo (excluyendo la escala actual)
        var existentes = await _escalaDataService.GetPagedAsync(new EscalaFiltroDataModel
        {
            IdVuelo = request.IdVuelo,
            PageNumber = 1,
            PageSize = 10000
        });

        if (existentes.Items.Any(x => x.IdEscala != idEscala && x.IdVuelo == request.IdVuelo && x.Orden == request.Orden))
            throw new BusinessException("Ya existe otra escala con el mismo orden para el vuelo indicado.");

        var dataModel = EscalaBusinessMapper.ToDataModel(idEscala, request);
        dataModel.ModificadoPorUsuario = modificadoPorUsuario;

        var actualizada = await _escalaDataService.UpdateAsync(dataModel);
        return actualizada == null ? null : EscalaBusinessMapper.ToResponseDto(actualizada);
    }

    public async Task<bool> DeleteAsync(int idEscala, string modificadoPorUsuario)
    {
        if (idEscala <= 0)
            throw new ValidationException("El id de la escala debe ser mayor que 0.");

        if (string.IsNullOrWhiteSpace(modificadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario modificador.");

        var actual = await _escalaDataService.GetByIdAsync(idEscala);
        if (actual == null)
            throw new NotFoundException("Escala no encontrada.");

        return await _escalaDataService.DeleteAsync(idEscala, modificadoPorUsuario);
    }
}