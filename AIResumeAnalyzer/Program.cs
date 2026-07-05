using AIResumeAnalyzer.Infrastructure.Data;
using AIResumeAnalyzer.Middleware;
using AIResumeAnalyzer.Services.Interfaces;
using AIResumeAnalyzer.Services.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var allowedOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>();


builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactPolicy", policy =>
    {
         policy.WithOrigins(allowedOrigins!)
        //policy.AllowAnyOrigin()
               .AllowAnyHeader()
               .AllowAnyMethod();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));

builder.Services.AddScoped<IAuthService, AuthService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

//add middleware
app.UseMiddleware<ExceptionMiddleware>();
app.UseCors("ReactPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
