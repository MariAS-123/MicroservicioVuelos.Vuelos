using Microservicio.Vuelos.Business.DTOs.Asiento;
using Microservicio.Vuelos.Business.Exceptions;
using Microservicio.Vuelos.Business.Interfaces;
using Microservicio.Vuelos.Business.Mappers;
using Microservicio.Vuelos.Business.Validators;
using Microservicio.Vuelos.DataManagement.Interfaces;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.Business.Services;

/// <summary>
/// Sin dependencias hacia otros MS — AsientoService solo usa
/// IVueloDataService y IAsientoDataService, ambos de BDD_Vuelos.
/// CAMBIO: DataPagedResult se construye con .Crear() en lugar de inicializador de objeto.
/// </summary>
public class AsientoService : IAsientoService
{
    private readonly IAsientoDataService _asientoDataService;
    private readonly IVueloDataService _vueloDataService;
    private readonly AsientoValidator _validator;

    public AsientoService(
        IAsientoDataService asientoDataService,
        IVueloDataService vueloDataService)
    {
        _asientoDataService = asientoDataService;
        _vueloDataService = vueloDataService;
        _validator = new AsientoValidator();
    }

    public async Task<DataPagedResult<AsientoResponseDto>> GetPagedAsync(AsientoFilterDto filter)
    {
        _validator.ValidateFilter(filter);

        var filtro = AsientoBusinessMapper.ToFiltroDataModel(filter);
        var result = await _asientoDataService.GetPagedAsync(filtro);

        return DataPagedResult<AsientoResponseDto>.Crear(
            AsientoBusinessMapper.ToResponseDtoList(result.Items),
            result.TotalRegistros,
            result.PaginaActual,
            result.TamanoPagina);
    }

    public async Task<AsientoResponseDto?> GetByIdAsync(int idAsiento)
    {
        if (idAsiento <= 0)
            throw new ValidationException("El id del asiento debe ser mayor que 0.");

        var data = await _asientoDataService.GetByIdAsync(idAsiento);
        return data == null ? null : AsientoBusinessMapper.ToResponseDto(data);
    }

    public async Task<AsientoResponseDto> CreateAsync(AsientoRequestDto request, string creadoPorUsuario)
    {
        if (string.IsNullOrWhiteSpace(creadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario creador.");

        _validator.ValidateRequest(request);

        var vuelo = await _vueloDataService.GetByIdAsync(request.IdVuelo);
        if (vuelo == null)
            throw new NotFoundException("El vuelo indicado no existe.");
        if (vuelo.Estado != "ACTIVO" || vuelo.EstadoVuelo is "CANCELADO" or "ATERRIZADO")
            throw new BusinessException("No se pueden crear asientos para un vuelo inactivo, cancelado o aterrizado.");

        var existentes = await _asientoDataService.GetPagedAsync(new AsientoFiltroDataModel
        {
            IdVuelo = request.IdVuelo,
            PageNumber = 1,
            PageSize = 10000
        });

        var numeroAsiento = request.NumeroAsiento.Trim().ToUpperInvariant();
        if (existentes.Items.Any(x => x.IdVuelo == request.IdVuelo &&
                                      x.NumeroAsiento.Trim().ToUpperInvariant() == numeroAsiento))
            throw new BusinessException("Ya existe un asiento con el mismo número en el vuelo indicado.");

        var dataModel = AsientoBusinessMapper.ToDataModel(request, creadoPorUsuario);
        var creado = await _asientoDataService.CreateAsync(dataModel);

        return AsientoBusinessMapper.ToResponseDto(creado);
    }

    public async Task<AsientoResponseDto?> UpdateAsync(int idAsiento, AsientoUpdateRequestDto request, string modificadoPorUsuario)
    {
        if (idAsiento <= 0)
            throw new ValidationException("El id del asiento debe ser mayor que 0.");

        if (string.IsNullOrWhiteSpace(modificadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario modificador.");

        _validator.ValidateUpdate(request);

        var actual = await _asientoDataService.GetByIdAsync(idAsiento);
        if (actual == null)
            throw new NotFoundException("Asiento no encontrado.");

        var vuelo = await _vueloDataService.GetByIdAsync(request.IdVuelo);
        if (vuelo == null)
            throw new NotFoundException("El vuelo indicado no existe.");
        if (vuelo.Estado != "ACTIVO" || vuelo.EstadoVuelo is "CANCELADO" or "ATERRIZADO")
            throw new BusinessException("No se pueden actualizar asientos para un vuelo inactivo, cancelado o aterrizado.");

        var existentes = await _asientoDataService.GetPagedAsync(new AsientoFiltroDataModel
        {
            IdVuelo = request.IdVuelo,
            PageNumber = 1,
            PageSize = 10000
        });

        var numeroAsiento = request.NumeroAsiento.Trim().ToUpperInvariant();
        if (existentes.Items.Any(x => x.IdAsiento != idAsiento &&
                                      x.IdVuelo == request.IdVuelo &&
                                      x.NumeroAsiento.Trim().ToUpperInvariant() == numeroAsiento))
            throw new BusinessException("Ya existe otro asiento con el mismo número en el vuelo indicado.");

        var dataModel = AsientoBusinessMapper.ToDataModel(idAsiento, request);
        dataModel.ModificadoPorUsuario = modificadoPorUsuario;

        var actualizado = await _asientoDataService.UpdateAsync(dataModel);
        return actualizado == null ? null : AsientoBusinessMapper.ToResponseDto(actualizado);
    }

    public async Task<bool> DeleteAsync(int idAsiento, string modificadoPorUsuario)
    {
        if (idAsiento <= 0)
            throw new ValidationException("El id del asiento debe ser mayor que 0.");

        if (string.IsNullOrWhiteSpace(modificadoPorUsuario))
            throw new UnauthorizedBusinessException("No se pudo identificar el usuario modificador.");

        var actual = await _asientoDataService.GetByIdAsync(idAsiento);
        if (actual == null)
            throw new NotFoundException("Asiento no encontrado.");

        return await _asientoDataService.DeleteAsync(idAsiento, modificadoPorUsuario);
    }
}