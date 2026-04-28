using System.Net.Http.Headers;
using Store.Models.Interfaces.Services;
using StoreUI.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.IsEssential = true;
});

// API HttpClient with JWT token handling
var apiBaseUrl = builder.Configuration["ApiSettings:BaseUrl"] ?? "https://localhost:7112";
builder.Services.AddHttpClient<IApiClientService, ApiClientService>(client =>
{
    client.BaseAddress = new Uri(apiBaseUrl);
    client.DefaultRequestHeaders.Add("Accept", "application/json");
});

// API service implementations
builder.Services.AddScoped<IAuthenticationService, ApiAuthenticationService>();
builder.Services.AddScoped<IUserService, ApiUserService>();
builder.Services.AddScoped<IEmployeeService, ApiEmployeeService>();
builder.Services.AddScoped<ICustomerService, ApiCustomerService>();
builder.Services.AddScoped<IItemService, ApiItemService>();
builder.Services.AddScoped<IInvoiceService, ApiInvoiceService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();
app.UseSession();

app.UseRouting();

app.MapRazorPages();

app.Run();

app.Run();
