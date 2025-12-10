using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Warehouse.Api.DTOs;
using Warehouse.Api.Domain.Entities;
using Warehouse.Api.Infrastructure.Persistence;
using Warehouse.Api.Domain.Abstractions;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanManageProducts")] // فقط Admin + WarehouseManager
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public CategoriesController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var categories = await _uow.Categories.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category is null)
            return NotFound();

        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Category name is required.");

        var exists = await _uow.Categories.NameExistsAsync(request.Name);
        if (exists)
            return BadRequest("Category with this name already exists.");

        var category = new Category
        {
            Name = request.Name.Trim()
        };

        await _uow.Categories.AddAsync(category);
        await _uow.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = category.Id }, category);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category is null)
            return NotFound();

        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Category name is required.");

        var nameExists = await _uow.Categories.NameExistsAsync(request.Name, id);
        if (nameExists)
            return BadRequest("Another category with this name already exists.");

        category.Name = request.Name.Trim();
        await _uow.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var category = await _uow.Categories.GetByIdAsync(id);
        if (category is null)
            return NotFound();

        var hasProducts = await _uow.Categories.HasProductsAsync(id);
        if (hasProducts)
            return BadRequest("Cannot delete category with products.");

        _uow.Categories.Remove(category);
        await _uow.SaveChangesAsync();

        return NoContent();
    }

}
