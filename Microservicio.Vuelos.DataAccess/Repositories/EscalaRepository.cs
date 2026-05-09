using Microsoft.EntityFrameworkCore;
using Microservicio.Vuelos.DataAccess.Context;
using Microservicio.Vuelos.DataAccess.Entities;
using Microservicio.Vuelos.DataAccess.Repositories.Interfaces;

namespace Microservicio.Vuelos.DataAccess.Repositories;

/// <summary>
/// Implementación del repositorio CRUD de escalas.
/// CAMBIOS MICROSERVICIO:
///   - Namespace y contexto actualizados (VuelosDbContext).
///   - Sin cambios de lógica — el repositorio solo toca vuelos.escala.
/// </summary>
public class EscalaRepository : IEscalaRepository
{
    private readonly VuelosDbContext _context;

    public EscalaRepository(VuelosDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<EscalaEntity>> ObtenerTodosAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Escalas
            .AsNoTracking()
            .Where(e => !e.Eliminado)
            .OrderBy(e => e.IdVuelo)
            .ThenBy(e => e.Orden)
            .ToListAsync(cancellationToken);
    }

    public async Task<EscalaEntity?> ObtenerPorIdAsync(int idEscala, CancellationToken cancellationToken = default)
    {
        return await _context.Escalas
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdEscala == idEscala && !e.Eliminado, cancellationToken);
    }

    // Sin AsNoTracking para que EF rastree cambios en Update/Delete
    public async Task<EscalaEntity?> ObtenerPorIdParaEditarAsync(int idEscala, CancellationToken cancellationToken = default)
    {
        return await _context.Escalas
            .FirstOrDefaultAsync(e => e.IdEscala == idEscala && !e.Eliminado, cancellationToken);
    }

    public async Task<IEnumerable<EscalaEntity>> ObtenerPorVueloAsync(int idVuelo, CancellationToken cancellationToken = default)
    {
        return await _context.Escalas
            .AsNoTracking()
            .Where(e => e.IdVuelo == idVuelo && !e.Eliminado)
            .OrderBy(e => e.Orden)
            .ToListAsync(cancellationToken);
    }

    public async Task<IEnumerable<EscalaEntity>> ObtenerPorAeropuertoAsync(int idAeropuerto, CancellationToken cancellationToken = default)
    {
        return await _context.Escalas
            .AsNoTracking()
            .Where(e => e.IdAeropuerto == idAeropuerto && !e.Eliminado)
            .OrderBy(e => e.IdVuelo)
            .ThenBy(e => e.Orden)
            .ToListAsync(cancellationToken);
    }

    public async Task<EscalaEntity?> ObtenerPorVueloYOrdenAsync(int idVuelo, int orden, CancellationToken cancellationToken = default)
    {
        return await _context.Escalas
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.IdVuelo == idVuelo && e.Orden == orden && !e.Eliminado, cancellationToken);
    }

    public async Task<bool> ExistePorIdAsync(int idEscala, CancellationToken cancellationToken = default)
    {
        return await _context.Escalas
            .AnyAsync(e => e.IdEscala == idEscala && !e.Eliminado, cancellationToken);
    }

    public async Task<bool> ExistePorVueloYOrdenAsync(int idVuelo, int orden, CancellationToken cancellationToken = default)
    {
        return await _context.Escalas
            .AnyAsync(e => e.IdVuelo == idVuelo && e.Orden == orden && !e.Eliminado, cancellationToken);
    }

    public async Task AgregarAsync(EscalaEntity entity, CancellationToken cancellationToken = default)
    {
        await _context.Escalas.AddAsync(entity, cancellationToken);
    }

    public void Actualizar(EscalaEntity entity)
    {
        _context.Escalas.Update(entity);
    }

    // Soft delete — nunca se borra físicamente
    public void Eliminar(EscalaEntity entity)
    {
        entity.Eliminado = true;
        _context.Escalas.Update(entity);
    }
}