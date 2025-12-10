using System.Threading.Tasks;

namespace Warehouse.Api.Domain.Abstractions;

public interface IUnitOfWork
{
    ICategoryRepository Categories { get; }
    IProductRepository Products { get; }
    IStockMovementRepository StockMovements { get; }

    Task<int> SaveChangesAsync();
}
