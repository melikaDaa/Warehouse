using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Domain.Abstractions;
using Warehouse.Api.Domain.Entities;

namespace Warehouse.Api.Infrastructure.Persistence.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly AppDbContext _db;

    public ProductRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Product>> GetAllWithCategoryAsync()
    {
        return await _db.Products
            .Include(p => p.Category)
            .OrderBy(p => p.Code)
            .ToListAsync();
    }

    public async Task<Product?> GetByIdWithCategoryAsync(int id)
    {
        return await _db.Products
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _db.Products.FindAsync(id);
    }

    public async Task<bool> CodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _db.Products.AsQueryable();

        query = query.Where(p => p.Code == code);

        if (excludeId.HasValue)
            query = query.Where(p => p.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task AddAsync(Product product)
    {
        await _db.Products.AddAsync(product);
    }

    public void Update(Product product)
    {
        _db.Products.Update(product);
    }

    public void Remove(Product product)
    {
        _db.Products.Remove(product);
    }
}
