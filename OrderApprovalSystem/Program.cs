using OrderApprovalSystem.DataAccess;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Add session support
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Configure SqlHelper as singleton
var connectionString = builder.Configuration.GetConnectionString("SiemensContext")
    ?? "Server=localhost\\SQLEXPRESS;Database=Siemens_Order_Approval;Trusted_Connection=True;TrustServerCertificate=True;";

builder.Services.AddSingleton(sp => SqlHelper.GetInstance(connectionString));

// Register repositories
builder.Services.AddScoped<OrderRepository>();
builder.Services.AddScoped<EmployeeRepository>();
builder.Services.AddScoped<ProductRepository>();
builder.Services.AddScoped<PartnerRepository>();
builder.Services.AddScoped<StockRepository>();
builder.Services.AddScoped<ReportRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseSession();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Account}/{action=Login}/{id?}")
    .WithStaticAssets();


app.Run();
