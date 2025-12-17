using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using StudentHelperAPI.Core.Abstractions;
using StudentHelperAPI.Features.Admin.AddGroup;
using StudentHelperAPI.Features.Admin.AddLectureOnSubject;
using StudentHelperAPI.Features.AI.Send;
using StudentHelperAPI.Features.Authentication.Auth;
using StudentHelperAPI.Features.Authentication.Reg;
using StudentHelperAPI.Features.User.Lectures.ReadLectureOnSubject;
using StudentHelperAPI.Features.User.Publications.AddPublication;
using StudentHelperAPI.Features.User.Publications.ReadCurrentPublications;
using StudentHelperAPI.Features.User.Publications.ReadPublications;
using StudentHelperAPI.Infrastructure.Services;
using StudentHelperAPI.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddOpenApi();
builder.Services.AddHttpContextAccessor();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Логирование
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// Database
builder.Services.AddDbContext<HelperDbContext>(options =>
    options.UseMySql(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        new MySqlServerVersion(new Version(8, 0, 0))
    ));

// MediatR
builder.Services.AddMediatR(cfg =>
    cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// AI Service - GigaChat
builder.Services.AddScoped<IAiService, GigaChatService>();
builder.Services.Configure<GigaChatSettings>(
    builder.Configuration.GetSection("GigaChat"));

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Регистрируем endpoints
app.SendMessageMap();
app.AuthMap();
app.RegMap();
app.GetLectureMap();
app.AddPublicationMap();
app.ReadPublicationsMap();
app.ReadCurrentPublicationsMap();
app.AddGroupMap();
app.AddLectureOnSubjectMap();
app.MapGet("/", () => "Student Helper API with GigaChat is running!");
app.MapGet("/health", () => "Healthy");

app.Run();