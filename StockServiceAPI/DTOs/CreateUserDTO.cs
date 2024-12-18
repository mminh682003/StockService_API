using System.ComponentModel.DataAnnotations;

namespace StockServiceAPI.DTOs
{
    public class CreateUserDTO
    {
        [Required(ErrorMessage = "Id là bắt buộc")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username không được để trống")]
        public string Username { get; set; }

        [Required(ErrorMessage = "Password không được để trống")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Email không được để trống")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        public string Email { get; set; }

        public string Fullname { get; set; }

    }
}
