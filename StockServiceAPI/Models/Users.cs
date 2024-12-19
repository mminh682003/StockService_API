
namespace StockServiceAPI.Models
{
    public class Users
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string Email { get; set; }
        public string Fullname { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? LastLogin { get; set; }
        public bool IsActive { get; set; } = true; // Giá trị mặc định: true
        public string Role { get; set; } = "User"; // Giá trị mặc định: "User"



        // Navigation properties
        public ICollection<Portfolios> Portfolio { get; set; } =new List<Portfolios>();
        public ICollection<Transactions> Transaction { get; set; }= new List<Transactions>();
    }
}
