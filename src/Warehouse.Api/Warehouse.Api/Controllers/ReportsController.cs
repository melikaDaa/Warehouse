using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.FeatureManagement;
using Warehouse.Api.Domain.Abstractions;
using Warehouse.Api.Infrastructure.Caching;

namespace Warehouse.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "CanViewReports")]
public class ReportsController : ControllerBase
{
    private readonly IUnitOfWork _uow;
    private readonly IFeatureManager _featureManager;
    private readonly ICacheService _cache;
    public ReportsController(IUnitOfWork uow, IFeatureManager featureManager, ICacheService cache = null)
    {
        _uow = uow;
        _featureManager = featureManager;
        _cache = cache;
    }

    // خلاصه موجودی همه‌ی کالاها
    [HttpGet("stock-summary")]
    public async Task<IActionResult> GetStockSummary()
    {
        if (!await _featureManager.IsEnabledAsync("EnableReports"))
            return NotFound();

        var cached = await _cache.GetAsync<List<object>>(CacheKeys.StockSummary);
        if (cached is not null)
            return Ok(cached);

        var products = await _uow.Products.GetAllWithCategoryAsync();

        var result = products.Select(p => new
        {
            p.Id,
            p.Code,
            p.Name,
            Category = p.Category.Name,
            p.CurrentStock
        }).OrderBy(x => x.Code).ToList<object>();

        await _cache.SetAsync(CacheKeys.StockSummary, result, TimeSpan.FromMinutes(2));

        return Ok(result);
    }

}
