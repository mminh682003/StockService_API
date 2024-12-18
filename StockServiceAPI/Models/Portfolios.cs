namespace StockServiceAPI.Models
{
    public class Portfolios
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StockId { get; set; }
        public int Quantity { get; set; }
        public decimal AveragePrice { get; set; }
        public DateTime DateAdded { get; set; }

        // Navigation properties
        public Users User { get; set; }
        public Stocks Stock { get; set; }
    }
}
