using StockServiceAPI.Models;
using Microsoft.EntityFrameworkCore;
using StockServiceAPI.Data;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace StockServiceAPI.Services
{
    public class UserService : IUserService
    {
        private readonly AppDbContext _context;

        public UserService(AppDbContext context)
        {
            _context = context;
        }

        // Thêm User
        public void AddUser(Users user)
        {
            user.DateCreated = DateTime.Now;
            user.LastLogin = DateTime.Now;
            _context.Users.Add(user);  
            _context.SaveChanges();    
        }
            
        // Kiểm tra xem Id User đã tồn tại hay chưa
        public bool UserExists(string name)
        {
            return _context.Users.Any(x => x.Username== name);  // Kiểm tra người dùng có tồn tại tên trong cơ sở dữ liệu không
        }

        public bool UserCheck(int id)
        {
            return _context.Users.Any(m=>m.Id == id); // Kiểm tra xem có tồn tại ID trong db không
        }
        public bool UserCheckPassword(Users user, string password)
        {
            if (user == null || string.IsNullOrEmpty(user.Password)) return false;
            // So sánh mật khẩu nhập vào với passhash trong cơ sở dữ liệu
            return BCrypt.Net.BCrypt.Verify(password, user.Password);
        }

        public async Task<Users> AuthenticateAsync(string UserName, string PassWord)
        {
            //Kiểm tra xem có user trong hệ thống hay không
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Username == UserName);
            //Nếu không tìm thấy user hoặc Pass không khớp sẽ trả về null
            if (user == null || !BCrypt.Net.BCrypt.Verify(PassWord, user?.Password ?? string.Empty)) {

                return null;
            
            }
            return user;
        }

        public async Task<bool> ResetPassword(Users user, string newPassword)
        {
            // Mã hóa mật khẩu mới
            var hashedNewPassword = BCrypt.Net.BCrypt.HashPassword(newPassword);

            // Gán mật khẩu đã mã hóa vào đối tượng user
            user.Password = hashedNewPassword;

            try
            {
                // Cập nhật thông tin người dùng
                _context.Users.Update(user);
                await _context.SaveChangesAsync();
                return true; // Thay đổi mật khẩu thành công
            }
            catch (Exception ex)
            {
                // Ghi log lỗi để hỗ trợ debug (nếu cần)
                Console.WriteLine($"lỗi khi đang resetpassword: {ex.Message}");
                return false; // Thay đổi mật khẩu thất bại
            }
        }

    }
}
