using Microservicio.Vuelos.Business.DTOs.Asiento;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.Business.Interfaces;

public interface IAsientoService
{
    Task<DataPagedResult<AsientoResponseDto>> GetPagedAsync(AsientoFilterDto filter);
    Task<AsientoResponseDto?> GetByIdAsync(int idAsiento);
    Task<AsientoResponseDto> CreateAsync(AsientoRequestDto request, string creadoPorUsuario);
    Task<AsientoResponseDto?> UpdateAsync(int idAsiento, AsientoUpdateRequestDto request, string modificadoPorUsuario);
    Task<bool> DeleteAsync(int idAsiento, string modificadoPorUsuario);
}