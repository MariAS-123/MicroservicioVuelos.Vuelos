using Microservicio.Vuelos.Business.DTOs.Escala;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.Business.Interfaces;

public interface IEscalaService
{
    Task<DataPagedResult<EscalaResponseDto>> GetPagedAsync(EscalaFilterDto filter);
    Task<EscalaResponseDto?> GetByIdAsync(int idEscala);
    Task<EscalaResponseDto> CreateAsync(EscalaRequestDto request, string creadoPorUsuario);
    Task<EscalaResponseDto?> UpdateAsync(int idEscala, EscalaUpdateRequestDto request, string modificadoPorUsuario);
    Task<bool> DeleteAsync(int idEscala, string modificadoPorUsuario);
}