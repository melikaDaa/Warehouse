namespace Warehouse.Api.Domain.Entities
{
    public class StockMovement
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = default!;

        public bool IsIn { get; set; }   // true = ورود، false = خروج
        public int Quantity { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? PerformedByUserId { get; set; }
    }
}
