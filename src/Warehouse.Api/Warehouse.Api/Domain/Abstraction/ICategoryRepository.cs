using Warehouse.Api.Domain.Entities;

namespace Warehouse.Api.Domain.Abstractions;

public interface ICategoryRepository
{
    Task<List<Category>> GetAllAsync();
    Task<Category?> GetByIdAsync(int id);
    Task<bool> NameExistsAsync(string name, int? excludeId = null);
    Task<bool> ExistsByIdAsync(int id);
    Task<bool> HasProductsAsync(int id);

    Task AddAsync(Category category);
    void Remove(Category category);
}
