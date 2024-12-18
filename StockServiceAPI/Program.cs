using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StockServiceAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


// Thêm DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddControllers();
// Configure the HTTP request pipeline.
var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();
app.Run();

