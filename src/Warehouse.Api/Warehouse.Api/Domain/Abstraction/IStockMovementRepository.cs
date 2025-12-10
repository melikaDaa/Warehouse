using Warehouse.Api.Domain.Entities;

namespace Warehouse.Api.Domain.Abstractions;

public interface IStockMovementRepository
{
    Task<bool> HasMovementsForProductAsync(int productId);

    Task AddAsync(StockMovement movement);
    Task<List<StockMovement>> GetByProductIdAsync(int productId);

}
