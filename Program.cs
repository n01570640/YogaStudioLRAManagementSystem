using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using YogaStudioLRAManagementSystem.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();
//oracle service
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseOracle(builder.Configuration.GetConnectionString("OracleConnection")));

//Add authentication
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.LoginPath = "/Auth/Login"; //redired to this path if not authenticated
        options.AccessDeniedPath = "/Auth/AccessDenied";//redirect to this path if access is denied
        options.ExpireTimeSpan = TimeSpan.FromMinutes(30); //Set cookies expiration time
    });
builder.Services.AddAuthorization();

//session to store JWT token after login so views can access it for API calls
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(8);  //session expires after 8 hours of inactivity
    options.Cookie.HttpOnly = true; //prevents JS from acceessing session cookie
    options.Cookie.IsEssential = true; //required
});


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseSession(); //must be fore authetnication - loads session data into request
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
