using Microsoft.EntityFrameworkCore;
using StoreProjectModels.DatabaseModels;
using StoreServices.Interfaces;
using StoreServices;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddDbContext<store_dbContext>(options =>
{
	options.UseMySQL(builder.Configuration.GetConnectionString("Default"));
});

builder.Services.AddScoped<ICategoryService, CategoryService>();

builder.Services.AddCors(p => p.AddPolicy("corspolicy", build =>
{
	build.WithOrigins("*").AllowAnyMethod().AllowAnyHeader();
}));
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{	
	app.UseSwagger();
	app.UseSwaggerUI();
}
app.UseCors("corspolicy");
app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
