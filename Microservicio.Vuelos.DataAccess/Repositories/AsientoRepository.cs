using Microsoft.EntityFrameworkCore;
using Microservicio.Vuelos.DataAccess.Context;
using Microservicio.Vuelos.DataAccess.Entities;
using Microservicio.Vuelos.DataAccess.Repositories.Interfaces;

namespace Microservicio.Vuelos.DataAccess.Repositories;

/// <summary>
/// Implementación del repositorio CRUD de asientos.
/// CAMBIOS MICROSERVICIO:
///   - Namespace y contexto actualizados (VuelosDbContext).
///   - Sin cambios de lógica — el repositorio solo toca vuelos.asiento.
/// </summary>
public class AsientoRepository : IAsientoRepository
{
    private readonly VuelosDbContext _context;

    public AsientoRepository(VuelosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<AsientoEntity>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Asientos
            .AsNoTracking()
            .Where(a => !a.Eliminado)
            .OrderBy(a => a.IdVuelo)
            .ThenBy(a => a.NumeroAsiento)
            .ToListAsync(cancellationToken);
    }

    public async Task<AsientoEntity?> ObtenerPorIdAsync(int idAsiento, CancellationToken cancellationToken = default)
    {
        return await _context.Asientos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdAsiento == idAsiento && !a.Eliminado, cancellationToken);
    }

    // Sin AsNoTracking para que EF rastree cambios en Update/Delete
    public async Task<AsientoEntity?> ObtenerPorIdParaEditarAsync(int idAsiento, CancellationToken cancellationToken = default)
    {
        return await _context.Asientos
            .FirstOrDefaultAsync(a => a.IdAsiento == idAsiento && !a.Eliminado, cancellationToken);
    }

    public async Task<IEnumerable<AsientoEntity>> ObtenerPorVueloAsync(int idVuelo, CancellationToken cancellationToken = default)
    {
        return await _context.Asientos
            .AsNoTracking()
            .Where(a => a.IdVuelo == idVuelo && !a.Eliminado)
            .OrderBy(a => a.NumeroAsiento)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<AsientoEntity>> ObtenerDisponiblesPorVueloAsync(int idVuelo, CancellationToken cancellationToken = default)
    {
        return await _context.Asientos
            .AsNoTracking()
            .Where(a => a.IdVuelo == idVuelo && a.Disponible && !a.Eliminado)
            .OrderBy(a => a.NumeroAsiento)
            .ToListAsync(cancellationToken);
    }

    public async Task<AsientoEntity?> ObtenerPorVueloYNumeroAsync(int idVuelo, string numeroAsiento, CancellationToken cancellationToken = default)
    {
        return await _context.Asientos
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.IdVuelo == idVuelo && a.NumeroAsiento == numeroAsiento && !a.Eliminado, cancellationToken);
    }

    public async Task<bool> ExistePorIdAsync(int idAsiento, CancellationToken cancellationToken = default)
    {
        return await _context.Asientos
            .AnyAsync(a => a.IdAsiento == idAsiento && !a.Eliminado, cancellationToken);
    }

    public async Task<bool> ExistePorVueloYNumeroAsync(int idVuelo, string numeroAsiento, CancellationToken cancellationToken = default)
    {
        return await _context.Asientos
            .AnyAsync(a => a.IdVuelo == idVuelo && a.NumeroAsiento == numeroAsiento && !a.Eliminado, cancellationToken);
    }

    public async Task AgregarAsync(AsientoEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.Asientos.AddAsync(entity, cancellationToken);
    }

    public void Actualizar(AsientoEntity entity)
    {
        _context.Asientos.Update(entity);
    }

    // Soft delete — nunca se borra físicamente
    public void Eliminar(AsientoEntity entity)
    {
        entity.Eliminado = true;
        _context.Asientos.Update(entity);
    }
}