namespace StockServiceAPI.Models
{
    public class Transactions
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int StockId { get; set; }
        public string TransactionType { get; set; } // Buy/Sell
        public int Quantity { get; set; }
        public decimal Price { get; set; }
        public decimal TotalAmount { get; set; }
        public DateTime TransactionDate { get; set; }

        // Navigation properties
        public Users User { get; set; }
        public Stocks Stock { get; set; }
    }
}
