namespace Warehouse.Api.DTOs;

public class CreateStockMovementRequest
{
    public int ProductId { get; set; }
    public bool IsIn { get; set; }   // true = ورود، false = خروج
    public int Quantity { get; set; }
}
