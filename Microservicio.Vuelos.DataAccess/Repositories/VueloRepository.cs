using Microsoft.EntityFrameworkCore;
using Microservicio.Vuelos.DataAccess.Context;
using Microservicio.Vuelos.DataAccess.Entities;
using Microservicio.Vuelos.DataAccess.Repositories.Interfaces;

namespace Microservicio.Vuelos.DataAccess.Repositories;

/// <summary>
/// Implementación del repositorio CRUD de vuelos.
/// CAMBIOS MICROSERVICIO:
///   - Namespace y contexto actualizados (VuelosDbContext en lugar de SistemaVuelosDBContext).
///   - Sin cambios de lógica — el repositorio solo toca vuelos.vuelo,
///     que sigue existiendo en esta BDD.
/// </summary>
public class VueloRepository : IVueloRepository
{
    private readonly VuelosDbContext _context;

    public VueloRepository(VuelosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<VueloEntity>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Vuelos
            .AsNoTracking()
            .Where(v => !v.Eliminado)
            .OrderBy(v => v.FechaHoraSalida)
            .ToListAsync(cancellationToken);
    }

    public async Task<VueloEntity?> ObtenerPorIdAsync(int idVuelo, CancellationToken cancellationToken = default)
    {
        return await _context.Vuelos
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.IdVuelo == idVuelo && !v.Eliminado, cancellationToken);
    }

    // Sin AsNoTracking para que EF rastree cambios en Update/Delete
    public async Task<VueloEntity?> ObtenerPorIdParaEditarAsync(int idVuelo, CancellationToken cancellationToken = default)
    {
        return await _context.Vuelos
            .FirstOrDefaultAsync(v => v.IdVuelo == idVuelo && !v.Eliminado, cancellationToken);
    }

    public async Task<VueloEntity?> ObtenerPorNumeroVueloAsync(string numeroVuelo, CancellationToken cancellationToken = default)
    {
        return await _context.Vuelos
            .AsNoTracking()
            .FirstOrDefaultAsync(v => v.NumeroVuelo == numeroVuelo && !v.Eliminado, cancellationToken);
    }

    public async Task<IEnumerable<VueloEntity>> ObtenerPorAeropuertoOrigenAsync(int idAeropuertoOrigen, CancellationToken cancellationToken = default)
    {
        return await _context.Vuelos
            .AsNoTracking()
            .Where(v => v.IdAeropuertoOrigen == idAeropuertoOrigen && !v.Eliminado)
            .OrderBy(v => v.FechaHoraSalida)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<VueloEntity>> ObtenerPorAeropuertoDestinoAsync(int idAeropuertoDestino, CancellationToken cancellationToken = default)
    {
        return await _context.Vuelos
            .AsNoTracking()
            .Where(v => v.IdAeropuertoDestino == idAeropuertoDestino && !v.Eliminado)
            .OrderBy(v => v.FechaHoraSalida)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistePorIdAsync(int idVuelo, CancellationToken cancellationToken = default)
    {
        return await _context.Vuelos
            .AnyAsync(v => v.IdVuelo == idVuelo && !v.Eliminado, cancellationToken);
    }

    public async Task<bool> ExistePorNumeroVueloAsync(string numeroVuelo, CancellationToken cancellationToken = default)
    {
        return await _context.Vuelos
            .AnyAsync(v => v.NumeroVuelo == numeroVuelo && !v.Eliminado, cancellationToken);
    }

    public async Task AgregarAsync(VueloEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.Vuelos.AddAsync(entity, cancellationToken);
    }

    public void Actualizar(VueloEntity entity)
    {
        _context.Vuelos.Update(entity);
    }

    // Soft delete — nunca se borra físicamente
    public void Eliminar(VueloEntity entity)
    {
        entity.Eliminado = true;
        _context.Vuelos.Update(entity);
    }
}