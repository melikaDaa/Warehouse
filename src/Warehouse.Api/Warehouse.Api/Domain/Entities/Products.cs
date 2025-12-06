namespace Warehouse.Api.Domain.Entities
{

    public class Product
    {
        public int Id { get; set; }
        public string Code { get; set; } = default!; // مثلا "PRD-001"
        public string Name { get; set; } = default!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = default!;

        // برای گزارش سریع
        public int CurrentStock { get; set; }
    }
}
