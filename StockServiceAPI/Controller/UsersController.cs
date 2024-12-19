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
    [Route("api/[controller]/[action]")]
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
        [HttpGet]
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
        [HttpGet("{id}")]
        public async Task<ActionResult<Users>> GetUsers(int id)
        {
            var users = await _context.Users.FindAsync(id);

            if (users == null)
            {
                return NotFound();
            }

            return users;
        }

        // PUT: api/Users/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutUsers(int id, Users users)
        {
            if (id != users.Id)
            {
                return BadRequest();
            }

            _context.Entry(users).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UsersExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }
        //Tạo user
        // POST: api/Users/add
        [HttpPost]
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
        [HttpPost]
        public async Task<IActionResult> Login([FromBody] LoginRequestDTO loginRequestDTO)
        {
            var user = await userService.AuthenticateAsync(loginRequestDTO.UserName, loginRequestDTO.PassWord);
            if (user == null)
            {
                return Unauthorized(new {message="Sai tài khoản hoặc mật khẩu!"});
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
        [HttpDelete]
        public async Task<IActionResult> DeleteUsers(int id)
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

        private bool UsersExists(int id)
        {
            return _context.Users.Any(e => e.Id == id);
        }
    }
}
