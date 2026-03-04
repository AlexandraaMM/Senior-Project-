using Microsoft.EntityFrameworkCore;
using PracticeFlow.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllersWithViews();
builder.Services.AddSession();


builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite("Data Source=PracticeFlow.db"));

var app = builder.Build();

app.UseStaticFiles();
app.UseRouting();
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}");

app.Run();