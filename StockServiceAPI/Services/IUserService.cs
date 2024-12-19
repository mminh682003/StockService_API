using StockServiceAPI.Models;
using System.Text.RegularExpressions;

namespace StockServiceAPI.Services
{
    public interface IUserService
    {
        bool UserExists(string name);  //Kiểm tra tên đã tồn tại trong hệ thống chưa
        bool UserCheck(int id);  // Hàm kiểm tra định dạng emails        
        Task<Users> AuthenticateAsync(string UserName, string PassWord);
        void AddUser(Users user);    //Thêm mới tài khoản
        public bool UserCheckPassword(Users user, string password);

        Task<bool> ResetPassword(Users user, string newPassword );

    }
}
