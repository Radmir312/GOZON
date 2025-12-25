namespace GOZON.Models
{
    public class Delivery
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public string SupplierName { get; set; }
        public string WarehouseName { get; set; }
        public int Quantity { get; set; }
        public string Date { get; set; }
    }
}
