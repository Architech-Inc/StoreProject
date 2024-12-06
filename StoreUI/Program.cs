using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using StoreServices.Interfaces;
using StoreServices;
using StoreUI.Data;
using StoreRazorClassLibrary;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();

// Add LibraryJsInterop to container
builder.Services.AddScoped<LibraryJsInterop>();

//Add Store services to the container
//builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
//builder.Services.AddScoped<ICategoryService, CategoryService>();
//builder.Services.AddScoped<IContextInit, ContextInit>();
//builder.Services.AddScoped<ICustomerService, CustomerService>();
//builder.Services.AddScoped<IDepartmentService, DepartmentService>();
//builder.Services.AddScoped<IEmployeeService, EmployeeService>();
//builder.Services.AddScoped<IInvoiceService, InvoiceService>();
//builder.Services.AddScoped<IItemService, ItemService>();
//builder.Services.AddScoped<IOrderService, OrderService>();
//builder.Services.AddScoped<ISalaryService, SalaryService>();
//builder.Services.AddScoped<ISaleService, SaleService>();
//builder.Services.AddScoped<ISupplierService, SupplierService>();
//builder.Services.AddScoped<IUnitService, UnitService>();
//builder.Services.AddScoped<IUserService, UserService>();

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

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
