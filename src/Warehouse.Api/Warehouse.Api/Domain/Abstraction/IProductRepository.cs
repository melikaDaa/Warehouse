using Warehouse.Api.Domain.Entities;

namespace Warehouse.Api.Domain.Abstractions;

public interface IProductRepository
{
    Task<List<Product>> GetAllWithCategoryAsync();
    Task<Product?> GetByIdWithCategoryAsync(int id);
    Task<Product?> GetByIdAsync(int id);
    Task<bool> CodeExistsAsync(string code, int? excludeId = null);

    Task AddAsync(Product product);
    void Update(Product product);
    void Remove(Product product);
}
