using StockServiceAPI.Models;
using Microsoft.EntityFrameworkCore;
using StockServiceAPI.Data;
using System.Text.RegularExpressions;

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
            _context.Users.Add(user);  // Sử dụng DbContext để thêm User vào cơ sở dữ liệu
            _context.SaveChanges();    // Lưu thay đổi vào cơ sở dữ liệu
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

    }
}
