using Company.Domain;
using Company.Domain.Repositories.Abstract;
using Company.Domain.Repositories.EntityFramework;
using Company.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// подключаем конфиг из appsetting.json
builder.Configuration.Bind("Project", new Config());

// подключаем нужный функционал приложения в качестве сервисов
builder.Services.AddTransient<ITextFieldsRepository, EFTextFieldsRepository>();  // привяхзка интерфейса к реализации
builder.Services.AddTransient<IServiceItemsRepository, EFServiceItemsRepository>();
builder.Services.AddTransient<DataManager>();

// подключаем корнтекст БД
builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(Config.ConnectionString));

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
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/accessdenied";
    options.SlidingExpiration = true;
});

//builder.Services.AddControllersWithViews();
builder.Services.AddControllersWithViews().SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0).AddSessionStateTempDataProvider();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// подключение поддержки статичееских файлов в приложении (css, js и тд)
app.UseStaticFiles();

// подключении системы маршрутизации
app.UseRouting();

// подключение аутентификацию и авторизациию
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

// регистрация нужнгых маршрутов (end-points)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
