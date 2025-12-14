using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Warehouse.Api.Domain.Abstractions;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanViewReports")]
public class ReportsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IFeatureManager _featureManager;

    public ReportsController(IUnitOfWork uow, IFeatureManager featureManager)
    {
        _uow = uow;
        _featureManager = featureManager;
    }

    // خلاصه موجودی همه‌ی کالاها
    [HttpGet("stock-summary")]
    public async Task<IActionResult> GetStockSummary()
    {
        if (!await _featureManager.IsEnabledAsync("EnableReports"))
            return NotFound(); // یا 403/404/501؛ برای تمرین 404 تمیز است
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
