using Warehouse.Api.Domain.Abstractions;

namespace Warehouse.Api.Infrastructure.Persistence;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _db;

    public ICategoryRepository Categories { get; }
    public IProductRepository Products { get; }
    public IStockMovementRepository StockMovements { get; }

    public UnitOfWork(
        AppDbContext db,
        ICategoryRepository categoryRepository,
        IProductRepository productRepository,
        IStockMovementRepository stockMovementRepository)
    {
        _db = db;
        Categories = categoryRepository;
        Products = productRepository;
        StockMovements = stockMovementRepository;
    }

    public Task<int> SaveChangesAsync()
    {
        return _db.SaveChangesAsync();
    }
}
