using Microservicio.Vuelos.DataAccess.Entities;

namespace Microservicio.Vuelos.DataAccess.Repositories.Interfaces;

/// <summary>
/// Contrato del repositorio de escalas.
/// Sin cambios de lógica — solo se actualizó el namespace.
/// </summary>
public interface IEscalaRepository
{
    Task<IEnumerable<EscalaEntity>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<EscalaEntity?> ObtenerPorIdAsync(int idEscala, CancellationToken cancellationToken = default);
    Task<EscalaEntity?> ObtenerPorIdParaEditarAsync(int idEscala, CancellationToken cancellationToken = default);
    Task<IEnumerable<EscalaEntity>> ObtenerPorVueloAsync(int idVuelo, CancellationToken cancellationToken = default);
    Task<IEnumerable<EscalaEntity>> ObtenerPorAeropuertoAsync(int idAeropuerto, CancellationToken cancellationToken = default);
    Task<EscalaEntity?> ObtenerPorVueloYOrdenAsync(int idVuelo, int orden, CancellationToken cancellationToken = default);
    Task<bool> ExistePorIdAsync(int idEscala, CancellationToken cancellationToken = default);
    Task<bool> ExistePorVueloYOrdenAsync(int idVuelo, int orden, CancellationToken cancellationToken = default);
    Task AgregarAsync(EscalaEntity entity, CancellationToken cancellationToken = default);
    void Actualizar(EscalaEntity entity);
    void Eliminar(EscalaEntity entity);
}