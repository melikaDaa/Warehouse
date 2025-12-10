using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.Domain.Abstractions;
using Warehouse.Api.Domain.Entities;
using Warehouse.Api.DTOs;
using Warehouse.Api.Infrastructure.Persistence;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] // همه اکشن‌ها نیاز به لاگین دارند، مگر جایی که AllowAnonymous بگذاریم
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public ProductsController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // GET: api/Products
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var products = await _uow.Products.GetAllWithCategoryAsync();

        var result = products.Select(p => new
        {
            p.Id,
            p.Code,
            p.Name,
            CategoryId = p.CategoryId,
            CategoryName = p.Category.Name,
            p.CurrentStock
        });

        return Ok(result);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var product = await _uow.Products.GetByIdWithCategoryAsync(id);
        if (product is null)
            return NotFound();

        return Ok(new
        {
            product.Id,
            product.Code,
            product.Name,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            product.CurrentStock
        });
    }

    [HttpPost]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Code is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var categoryExists = await _uow.Categories.ExistsByIdAsync(request.CategoryId);
        if (!categoryExists)
            return BadRequest("Category not found.");

        var codeExists = await _uow.Products.CodeExistsAsync(request.Code);
        if (codeExists)
            return BadRequest("Product code already exists.");

        var product = new Product
        {
            Code = request.Code.Trim(),
            Name = request.Name.Trim(),
            CategoryId = request.CategoryId,
            CurrentStock = 0
        };

        await _uow.Products.AddAsync(product);
        await _uow.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpPut("{id:int}")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> Update(int id, [FromBody] CreateProductRequest request)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.Code))
            return BadRequest("Code is required.");

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");

        var categoryExists = await _uow.Categories.ExistsByIdAsync(request.CategoryId);
        if (!categoryExists)
            return BadRequest("Category not found.");

        var codeExists = await _uow.Products.CodeExistsAsync(request.Code, id);
        if (codeExists)
            return BadRequest("Another product with this code already exists.");

        product.Code = request.Code.Trim();
        product.Name = request.Name.Trim();
        product.CategoryId = request.CategoryId;

        await _uow.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [Authorize(Policy = "CanManageProducts")]
    public async Task<IActionResult> Delete(int id)
    {
        var product = await _uow.Products.GetByIdAsync(id);
        if (product is null)
            return NotFound();

        var hasMovements = await _uow.StockMovements.HasMovementsForProductAsync(id);
        if (hasMovements)
            return BadRequest("Cannot delete product with stock movements.");

        _uow.Products.Remove(product);
        await _uow.SaveChangesAsync();

        return NoContent();
    }
    // گزارش گردش یک کالا
    [HttpGet("{id:int}/stock-movements")]
    [Authorize(Policy = "CanViewReports")]
    public async Task<IActionResult> GetStockMovements(int id)
    {
        var product = await _uow.Products.GetByIdWithCategoryAsync(id);
        if (product is null)
            return NotFound("Product not found.");

        var movements = await _uow.StockMovements.GetByProductIdAsync(id);

        var response = new
        {
            productId = product.Id,
            productCode = product.Code,
            productName = product.Name,
            category = product.Category?.Name,
            currentStock = product.CurrentStock,
            movements = movements.Select(m => new
            {
                m.Id,
                m.IsIn,
                m.Quantity,
                m.Timestamp,
                m.PerformedByUserId
            })
        };

        return Ok(response);
    }



}
