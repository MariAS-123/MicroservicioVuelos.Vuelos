using Microservicio.Vuelos.Business.DTOs.Vuelo;
using Microservicio.Vuelos.DataManagement.Models;

namespace Microservicio.Vuelos.Business.Interfaces;

public interface IVueloService
{
    Task<DataPagedResult<VueloResponseDto>> GetPagedAsync(VueloFilterDto filter);
    Task<VueloResponseDto?> GetByIdAsync(int idVuelo);
    Task<VueloResponseDto> CreateAsync(VueloRequestDto request, string creadoPorUsuario);
    Task<VueloResponseDto?> UpdateAsync(int idVuelo, VueloUpdateRequestDto request, string modificadoPorUsuario);
    Task<VueloResponseDto?> UpdateEstadoAsync(int idVuelo, VueloEstadoRequestDto request, string modificadoPorUsuario);
    Task<bool> DeleteAsync(int idVuelo, string modificadoPorUsuario);
    Task<DataPagedResult<VueloResponseDto>> GetPagedBookingAsync(VueloFilterDto filter);
}