using Company.Domain;
using Company.Domain.Repositories.Abstract;
using Company.Domain.Repositories.EntityFramework;
using Company.Service;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// ���������� ������ �� appsetting.json
builder.Configuration.Bind("Project", new Config());

// ���������� ������ ���������� ���������� � �������� ��������
builder.Services.AddTransient<ITextFieldsRepository, EFTextFieldsRepository>();  // ��������� ���������� � ����������
builder.Services.AddTransient<IServiceItemsRepository, EFServiceItemsRepository>();
builder.Services.AddTransient<DataManager>();

// ���������� ��������� ��
builder.Services.AddDbContext<AppDbContext>(x => x.UseSqlServer(Config.ConnectionString));

// ����������� identity ������� 
builder.Services.AddIdentity<IdentityUser, IdentityRole> (opts =>  // ���������� ��� �����
{
    opts.User.RequireUniqueEmail = true;
    opts.Password.RequiredLength = 6;
    opts.Password.RequireNonAlphanumeric = false;
    opts.Password.RequireLowercase = false;
    opts.Password.RequireUppercase = false;
    opts.Password.RequireDigit = false;
}).AddEntityFrameworkStores<AppDbContext>().AddDefaultTokenProviders();

// ����������� ������� ��� ������������ � ������������� (MVC)
builder.Services.ConfigureApplicationCookie(options =>
{
    options.Cookie.Name = "myCompanyAuth";
    options.Cookie.HttpOnly = true;
    options.LoginPath = "/account/login";
    options.AccessDeniedPath = "/account/accessdenied";
    options.SlidingExpiration = true;
});

// ����������� �������� ����������� ��� Admin area
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("AdminArea", policy => { policy.RequireRole("admin"); });
});

// ��������� ������� ��� ������������ � ������������� (MVC)                                                          // ��������� ������������� � asp.net core 3.0
builder.Services.AddControllersWithViews(x =>
{
    x.Conventions.Add(new AdminAreaAuthorization("Admin", "AdminArea"));
})
  .SetCompatibilityVersion(Microsoft.AspNetCore.Mvc.CompatibilityVersion.Version_3_0).AddSessionStateTempDataProvider();


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}

// ����������� ��������� ������������ ������ � ���������� (css, js � ��)
app.UseStaticFiles();

// ����������� ������� �������������
app.UseRouting();

// ����������� �������������� � ������������
app.UseCookiePolicy();
app.UseAuthentication();
app.UseAuthorization();

// ����������� ������� ��������� (end-points)
app.MapControllerRoute( name: "admin", pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
app.MapControllerRoute( name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
