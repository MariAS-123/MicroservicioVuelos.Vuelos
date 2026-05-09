using Microservicio.Vuelos.DataAccess.Entities;

namespace Microservicio.Vuelos.DataAccess.Repositories.Interfaces;

/// <summary>
/// Contrato del repositorio de vuelos.
/// Sin cambios de lógica — solo se actualizó el namespace.
/// </summary>
public interface IVueloRepository
{
    Task<IEnumerable<VueloEntity>> ObtenerTodosAsync(CancellationToken cancellationToken = default);
    Task<VueloEntity?> ObtenerPorIdAsync(int idVuelo, CancellationToken cancellationToken = default);
    Task<VueloEntity?> ObtenerPorIdParaEditarAsync(int idVuelo, CancellationToken cancellationToken = default);
    Task<VueloEntity?> ObtenerPorNumeroVueloAsync(string numeroVuelo, CancellationToken cancellationToken = default);
    Task<IEnumerable<VueloEntity>> ObtenerPorAeropuertoOrigenAsync(int idAeropuertoOrigen, CancellationToken cancellationToken = default);
    Task<IEnumerable<VueloEntity>> ObtenerPorAeropuertoDestinoAsync(int idAeropuertoDestino, CancellationToken cancellationToken = default);
    Task<bool> ExistePorIdAsync(int idVuelo, CancellationToken cancellationToken = default);
    Task<bool> ExistePorNumeroVueloAsync(string numeroVuelo, CancellationToken cancellationToken = default);
    Task AgregarAsync(VueloEntity entity, CancellationToken cancellationToken = default);
    void Actualizar(VueloEntity entity);
    void Eliminar(VueloEntity entity);
}