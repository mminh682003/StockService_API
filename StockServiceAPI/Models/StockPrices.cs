namespace StockServiceAPI.Models
{
    public class StockPrices
    {
        public int Id { get; set; }
        public int StockId { get; set; }
        public decimal Price { get; set; }
        public DateTime DateTime { get; set; }

        // Navigation property
        public Stocks Stock { get; set; }
    }
}
