using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StockServiceAPI.Data;
using StockServiceAPI.DTOs;
using StockServiceAPI.Models;
using StockServiceAPI.Services;
using System.Text.RegularExpressions;
using System.Net;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace StockServiceAPI.Controller
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserService userService;
        private readonly IConfiguration configuration;

        public UsersController(AppDbContext context, IUserService userService, IConfiguration configuration)
        {
            _context = context;
            this.userService = userService;
            this.configuration = configuration;

        }

        // GET: api/Users
        [HttpGet("list-users")]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }

        //GET: api/users/timkiem?=name
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetbyName()
        {


            return await _context.Users.ToListAsync();
        }


        // GET: api/Users/5
        [HttpGet("user")]
        public async Task<ActionResult<Users>> GetUsers([FromQuery]int id)
        {
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return users;
        }

        // PUT: api/Users/5
        [HttpPut("update_profile")]
        public async Task<IActionResult> UpdateProfile([FromQuery] int id, [FromBody] UpdateProfileRequestDTO updateProfileRequestDTO)
        {
            // Kiểm tra ID có hợp lệ không
            if (id <= 0)
            {
                return BadRequest(new { message = "ID không hợp lệ." });
            }

            // Kiểm tra xem User có tồn tại không
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy User." });
            }

            // Kiểm tra Email trùng lặp
            if (_context.Users.Any(u => u.Email == updateProfileRequestDTO.Email && u.Id != id))
            {
                return Conflict(new { message = "Email đã tồn tại." });
            }

            // Cập nhật thông tin
            user.Fullname = updateProfileRequestDTO.FullName ?? user.Fullname;
            user.Email = updateProfileRequestDTO.Email ?? user.Email;

            try
            {
                _context.Entry(user).State = EntityState.Modified;
                await _context.SaveChangesAsync();
                return Ok(new { message = "Cập nhật thông tin thành công." });
            }
            catch (DbUpdateConcurrencyException ex)
            {
                // Log lỗi để debug (nếu cần)
                return StatusCode(500, new { message = "Lỗi khi cập nhật thông tin, vui lòng thử lại.", error = ex.Message });
            }
        }

        [HttpPut("reset-password")]
        public async Task<IActionResult> ResetPassword([FromQuery] int id, [FromBody] ResetPassworDTO resetPassworDTO)
        {
            if (id <= 0)
            {
                return BadRequest(new { message = "ID không hợp lệ." });
            }
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "Không tìm thấy User." });
            }
            //Check dữ liệu từ Client
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            //Check user có tồn tại hay không
            if (!userService.UserCheckPassword(user, resetPassworDTO.CurrentPassword))
            {
                return Conflict(new { message = "Sai mật khẩu cũ. Vui lòng nhập lại" });
            }
            if (resetPassworDTO.CurrentPassword == resetPassworDTO.NewPassword)
            {
                return BadRequest("Mật khẩu mới và mật khẩu cũ không được giống nhau");
            }
            var result = await userService.ResetPassword(user, resetPassworDTO.NewPassword);
            if (!result)
            {
                return StatusCode(500, "Thay đổi mật khẩu thất bại");
            }
            return  Ok(new { message = "Cập nhật mật khẩu thành công" });
        }



 //Tạo user
 // POST: api/Users/add
 [HttpPost("add-users")]
        public async Task<ActionResult<Users>> AddUser([FromBody] CreateUserDTO newUser)
        {
            // Validate dữ liệu
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // Trả về tất cả lỗi validation nếu có

            // Kiểm tra xem ID đã tồn tại chưa
            if (userService.UserCheck(newUser.Id))
            {
                return Conflict(new { message = "Tài khoản đã tồn tại trong hệ thống." });
            }
            // Kiểm tra xem Email đã tồn tại chưa
            if (_context.Users.Any(u => u.Email == newUser.Email))
            {
                return Conflict(new { message = "Email đã tồn tại trong hệ thống." });
            }

            //Băm mật khẩu
            var HashPassWord = BCrypt.Net.BCrypt.HashPassword(newUser.Password);

            // Tạo mới đối tượng User từ DTO
            var user = new Users
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Password = HashPassWord,
                Email = newUser.Email,
                Fullname = newUser.Fullname,
                DateCreated = DateTime.Now,
                IsActive = true,
                Role = "User"
            };

            // Thêm mới User vào database
            userService.AddUser(user);

            return Ok( new { message = "Tạo tài khoản thành công!", data = user });
        }

        //Đăng nhập
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user = await userService.AuthenticateAsync(loginRequestDTO.UserName, loginRequestDTO.PassWord);
            if (user == null)
            {
                return Unauthorized(new {message="Sai tài khoản hoặc mật khẩu!"});
            }
            if (user.IsActive == false || user.IsActive == null)
            {
                return Unauthorized(new {message="Người dùng không được phép truy cập hệ thống"});
            }
           //Tạo JWT token
            var tokenHandler =  new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"]);
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject =new ClaimsIdentity(    
                    new Claim[]
                    {
                        new Claim(ClaimTypes.Name, user.Username),
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Role, user.Role)
                    }),
                  Expires =DateTime.UtcNow.AddHours(1), // Token có thời hạn 1 tiếng
                  Issuer = configuration["Jwt:Issuer"],
                  Audience = configuration["Jwt:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);
            return Ok(new {Token = tokenString});
        }


        // DELETE: api/Users/5
        [HttpDelete("delete-users")]
        public async Task<IActionResult> DeleteUsers([FromQuery] int id)
        {
            // Kiểm tra xem người dùng có tồn tại hay không
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                // Nếu không tìm thấy người dùng, trả về NotFound
                return NotFound(new { message = "User không tồn tại." });
            }

            // Nếu người dùng tồn tại, xóa người dùng
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            // Trả về thông báo thành công
            return Ok(new { message = "Xóa user thành công!" });
        }

    }
}
