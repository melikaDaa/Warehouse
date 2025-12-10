using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Domain.Abstractions;
using Warehouse.Api.Domain.Entities;

namespace Warehouse.Api.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db)
    {
        _db = db;
    }

    public async Task<List<Category>> GetAllAsync()
    {
        return await _db.Categories
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _db.Categories.FindAsync(id);
    }

    public async Task<bool> NameExistsAsync(string name, int? excludeId = null)
    {
        var query = _db.Categories.AsQueryable();

        query = query.Where(c => c.Name == name);

        if (excludeId.HasValue)
            query = query.Where(c => c.Id != excludeId.Value);

        return await query.AnyAsync();
    }

    public async Task<bool> ExistsByIdAsync(int id)
    {
        return await _db.Categories.AnyAsync(c => c.Id == id);
    }

    public async Task<bool> HasProductsAsync(int id)
    {
        return await _db.Products.AnyAsync(p => p.CategoryId == id);
    }

    public async Task AddAsync(Category category)
    {
        await _db.Categories.AddAsync(category);
    }

    public void Remove(Category category)
    {
        _db.Categories.Remove(category);
    }
}
