using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.DataManagement.Interfaces;

/// <summary>
/// Contrato del servicio de datos de vuelos.
/// Sin cambios respecto al monolito.
/// </summary>
public interface IVueloDataService
{
    Task<DataPagedResult<VueloDataModel>> GetPagedAsync(VueloFiltroDataModel filtro);
    Task<VueloDataModel?> GetByIdAsync(int id);
    Task<VueloDataModel> CreateAsync(VueloDataModel model);
    Task<VueloDataModel?> UpdateAsync(VueloDataModel model);
    Task<bool> DeleteAsync(int id, string modificadoPorUsuario);
}