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

namespace StockServiceAPI.Controller
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IUserService userService;

        public UsersController(AppDbContext context, IUserService userService)
        {
            _context = context;
            this.userService = userService;
        }

        // GET: api/Users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Users>>> GetUsers()
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

        // POST: api/Users/add
        [HttpPost]
        public async Task<ActionResult<Users>> AddUser([FromBody] CreateUserDTO newUser)
        {
            // Validate dữ liệu
            if (!ModelState.IsValid)
                return BadRequest(ModelState); // Trả về tất cả lỗi validation nếu có

            // Kiểm tra xem ID đã tồn tại chưa
            if (userService.UserExists(newUser.Username))
            {
                return Conflict(new { message = "Tên đã tồn tại trong hệ thống." });
            }
            // Kiểm tra xem Email đã tồn tại chưa
            if (_context.Users.Any(u => u.Email == newUser.Email))
            {
                return Conflict(new { message = "Email đã tồn tại trong hệ thống." });
            }
            // Tạo mới đối tượng User từ DTO
            var user = new Users
            {
                Id = newUser.Id,
                Username = newUser.Username,
                Password = newUser.Password,
                Email = newUser.Email,
                Fullname = newUser.Fullname,
                DateCreated = DateTime.Now
            };

            // Thêm mới User vào database
            userService.AddUser(user);

            return Ok( new { message = "Thêm mới User thành công!", data = user });
        }


        // POST: api/Users
        [HttpPost]
        public async Task<ActionResult<Users>> PostUsers(Users users)
        {
            _context.Users.Add(users);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetUsers", new { id = users.Id }, users);
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
