using Company.Domain;
using Company.Domain.Repositories.Abstract;
using Company.Domain.Repositories.EntityFramework;
using Company.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.Bind("Project", new Config());  // подключаем конфиг из appsetting.json

// подключаем нужный функционал приложения в качестве сервисов
builder.Services.AddTransient<ITextFieldsRepository, EFTextFieldsRepository>();  // привяхзка интерфейса к реализации
builder.Services.AddTransient<IServiceItemsRepository, EFServiceItemsRepository>();
builder.Services.AddTransient<DataManager>();

builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(Config.ConnectionString));  // подключаем корнтекст БД

// настраиваем identity систему 
builder.Services.AddIdentity<IdentityUser, IdentityRole> (opts =>  // требования для входа
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequiredLength = 6;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireDigit = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// настраиваем сервисы для контроллеров и представлений (MVC)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "myCompanyAuth";
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/account/login";  // путь для залогина пользователя
    options.AccessDeniedPath = "/account/accessdenied";
    options.SlidingExpiration = true;
});

// настраиваем политику авторизации для Admin area
builder.Services.AddAuthorization(x =>
{
    x.AddPolicy("AdminArea", policy => { policy.RequireRole("admin"); }); // добавляем политику и требуем от пользователя роль "admin"
});

// добавляем сервисы для контроллеров и представлений (MVC)
builder.Services.AddControllersWithViews(x =>
{
    x.Conventions.Add(new AdminAreaAuthorization("Admin", "AdminArea"));  // Admin - область, AdminArea - политика которая будет действовать для этой области (для )
})
  .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0).AddSessionStateTempDataProvider();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{ 
    app.UseDeveloperExceptionPage();
}

app.UseStaticFiles();  // подключение поддержки статичееских файлов в приложении (css, js и тд)
app.UseRouting();  // подключении системы маршрутизации

// подключение аутентификацию и авторизациию
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute( name: "admin", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");  // регистрация нужных маршрутов (end-points)
app.MapControllerRoute( name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();