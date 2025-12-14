using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Warehouse.Api.Domain.Abstractions;
using Warehouse.Api.Domain.Entities;
using Warehouse.Api.DTOs;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StockMovementsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IFeatureManager _featureManager;

    public StockMovementsController(IUnitOfWork uow, IFeatureManager featureManager)
    {
        _uow = uow;
        _featureManager = featureManager;
    }

    // ثبت ورود/خروج کالا
    [HttpPost]
    [Authorize(Policy = "CanDoStockMovements")]
    public async Task<IActionResult> Create([FromBody] CreateStockMovementRequest request)
    {
        if (!await _featureManager.IsEnabledAsync("EnableStockMovements"))
            return NotFound();
        if (request.Quantity <= 0)
            return BadRequest("Quantity must be greater than zero.");

        var product = await _uow.Products.GetByIdAsync(request.ProductId);
        if (product is null)
            return NotFound("Product not found.");

        // محاسبه موجودی جدید
        var newStock = request.IsIn
            ? product.CurrentStock + request.Quantity
            : product.CurrentStock - request.Quantity;

        if (!request.IsIn && newStock < 0)
            return BadRequest("Stock cannot be negative.");

        // گرفتن UserId از توکن
        var userId = User.FindFirstValue(JwtRegisteredClaimNames.Sub);

        var movement = new StockMovement
        {
            ProductId = product.Id,
            IsIn = request.IsIn,
            Quantity = request.Quantity,
            Timestamp = DateTime.UtcNow,
            PerformedByUserId = userId
        };

        await _uow.StockMovements.AddAsync(movement);

        product.CurrentStock = newStock;

        await _uow.SaveChangesAsync();

        return Ok(new
        {
            message = "Stock movement recorded.",
            productId = product.Id,
            newStock = product.CurrentStock,
            isIn = request.IsIn,
            quantity = request.Quantity
        });
    }
}
