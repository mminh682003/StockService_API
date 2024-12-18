using StockServiceAPI.Models;
using System.Text.RegularExpressions;

namespace StockServiceAPI.Services
{
    public interface IUserService
    {
        bool UserExists(string name);
        void AddUser(Users user);
        // Hàm kiểm tra định dạng email

        bool UserCheck(int id);
    }
}
