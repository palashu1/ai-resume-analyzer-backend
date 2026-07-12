using AIResumeAnalyzer.DTO;
using AIResumeAnalyzer.Infrastructure.Data;
using AIResumeAnalyzer.Middleware;
using AIResumeAnalyzer.Services.BackgroundServices;
using AIResumeAnalyzer.Services.Interfaces;
using AIResumeAnalyzer.Services.Services;
using Microsoft.AspNetCore.HttpOverrides;
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
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<ApplicationDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("defaultConnection")));

builder.Services.Configure<JwtSettingsDto>(
    builder.Configuration.GetSection("Jwt"));
//For we know which device use by endUser
builder.Services.AddHttpContextAccessor();
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        ForwardedHeaders.XForwardedFor |
        ForwardedHeaders.XForwardedProto;
});

//background service
builder.Services.AddHostedService<RefreshTokenCleanupService>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IRefreshTokenService, RefreshTokenService>();
builder.Services.AddScoped<IRequestInfoService, RequestInfoService>();


var app = builder.Build();
app.UseForwardedHeaders();

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
