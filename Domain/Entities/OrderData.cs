namespace Domain.Entities
{
    public class OrderData
    {
        public string ItemName { get; set; } = string.Empty;
        public int AmountOrdered { get; set; }
        public decimal Price { get; set; }
    }
}
