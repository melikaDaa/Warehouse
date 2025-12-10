using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Domain.Abstractions;
using Warehouse.Api.Domain.Entities;

namespace Warehouse.Api.Infrastructure.Persistence.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly AppDbContext _db;

    public StockMovementRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> HasMovementsForProductAsync(int productId)
    {
        return await _db.StockMovements.AnyAsync(m => m.ProductId == productId);
    }

    public async Task AddAsync(StockMovement movement)
    {
        await _db.StockMovements.AddAsync(movement);
    }

    public async Task<List<StockMovement>> GetByProductIdAsync(int productId)
    {
        return await _db.StockMovements
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.Timestamp)
            .ToListAsync();
    }
}
