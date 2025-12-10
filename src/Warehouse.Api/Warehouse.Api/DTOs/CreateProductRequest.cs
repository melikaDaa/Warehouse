namespace Warehouse.Api.DTOs;

public class CreateProductRequest
{
    public string Code { get; set; } = default!;
    public string Name { get; set; } = default!;
    public int CategoryId { get; set; }
}
