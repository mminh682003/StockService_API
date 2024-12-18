

namespace StockServiceAPI.Models
{
    public class Stocks
    {
        public int Id { get; set; }
        public string Symbol { get; set; }
        public string Name { get; set; }
        public string Sector { get; set; }
        public decimal MaketCap { get; set; }
        public DateTime CreatedAt { get; set; }

        // Navigation properties
        public ICollection<StockPrices> StockPrice { get; set; }
        public ICollection<Portfolios> Portfolio { get; set; }
        public ICollection<Transactions> Transaction { get; set; }
    }
}
