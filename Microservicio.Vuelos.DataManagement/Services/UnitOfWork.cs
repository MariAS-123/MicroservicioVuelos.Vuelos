using Microservicio.Vuelos.DataAccess.Context;
using Microservicio.Vuelos.DataManagement.Interfaces;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microservicio.Vuelos.DataManagement.Services;

/// <summary>
/// CAMBIO MICROSERVICIO: SistemaVuelosDBContext → VuelosDbContext
/// Solo contiene las 3 tablas de BDD_Vuelos.
/// </summary>
public class UnitOfWork : IUnitOfWork
{
    private readonly VuelosDbContext _context;
    private IDbContextTransaction? _currentTransaction;

    public UnitOfWork(VuelosDbContext context)
    {
        _context = context;
    }

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            return;

        _currentTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            return;

        await _currentTransaction.CommitAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is null)
            return;

        await _currentTransaction.RollbackAsync(cancellationToken);
        await _currentTransaction.DisposeAsync();
        _currentTransaction = null;
    }

    public async Task ExecuteInTransactionAsync(Func<Task> operation, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
        {
            await operation();
            return;
        }

        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync<int, bool>(
            state: 0,
            operation: async (_, _, ct) =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);
                await operation();
                await transaction.CommitAsync(ct);
                return true;
            },
            verifySucceeded: null,
            cancellationToken: cancellationToken);
    }

    public async Task<T> ExecuteInTransactionAsync<T>(Func<Task<T>> operation, CancellationToken cancellationToken = default)
    {
        if (_currentTransaction is not null)
            return await operation();

        var strategy = _context.Database.CreateExecutionStrategy();
        return await strategy.ExecuteAsync<int, T>(
            state: 0,
            operation: async (_, _, ct) =>
            {
                await using var transaction = await _context.Database.BeginTransactionAsync(ct);
                var result = await operation();
                await transaction.CommitAsync(ct);
                return result;
            },
            verifySucceeded: null,
            cancellationToken: cancellationToken);
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}