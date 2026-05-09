using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.DataManagement.Interfaces;

/// <summary>
/// Contrato del servicio de datos de escalas.
/// Sin cambios respecto al monolito.
/// </summary>
public interface IEscalaDataService
{
    Task<DataPagedResult<EscalaDataModel>> GetPagedAsync(EscalaFiltroDataModel filtro);
    Task<EscalaDataModel?> GetByIdAsync(int id);
    Task<EscalaDataModel> CreateAsync(EscalaDataModel model);
    Task<EscalaDataModel?> UpdateAsync(EscalaDataModel model);
    Task<bool> DeleteAsync(int id, string modificadoPorUsuario);
}