using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Warehouse.Api.Domain.Abstractions;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanViewReports")]
public class ReportsController : ControllerBase
{
    private readonly IUnitOfWork _uow;

    public ReportsController(IUnitOfWork uow)
    {
        _uow = uow;
    }

    // خلاصه موجودی همه‌ی کالاها
    [HttpGet("stock-summary")]
    public async Task<IActionResult> GetStockSummary()
    {
        var products = await _uow.Products.GetAllWithCategoryAsync();

        var result = products
            .Select(p => new
            {
                p.Id,
                p.Code,
                p.Name,
                Category = p.Category.Name,
                p.CurrentStock
            })
            .OrderBy(p => p.Code)
            .ToList();

        return Ok(result);
    }
}
