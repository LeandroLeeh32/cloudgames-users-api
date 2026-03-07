using CloudGames.Users.Application.Interfaces.Messaging;
using CloudGames.Users.Application.Interfaces.Security;
using CloudGames.Users.Infrastructure.Messaging.Configuration;
using CloudGames.Users.Infrastructure.Messaging.EventBus;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using NLog;
using NLog.Web;
using System.Text;
using Users.API.Middlewares;
using Users.Application.Interfaces.Repositories;
using Users.Application.Security;
using Users.Application.UseCases.Auth;
using Users.Application.UseCases.Users;
using Users.Infrastructure.Persistence.Context;
using Users.Infrastructure.Repositories;
using Users.Infrastructure.Security;
using Users.Infrastructure.Seed;

#region LOGGER

var logger = LogManager
    .Setup()
    .LoadConfigurationFromFile("nlog.config")
    .GetCurrentClassLogger();

#endregion

#region BUILDER

var builder = WebApplication.CreateBuilder(args);

builder.Logging.ClearProviders();
builder.Host.UseNLog();

#endregion

#region MASSTRANSIT   

#region MASSTRANSIT

var rabbitSettings = builder.Configuration.GetSection("RabbitMQ").Get<RabbitMqSettings>();

builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.Host(
            rabbitSettings!.Host,
            rabbitSettings.VirtualHost,
            h =>
            {
                h.Username(rabbitSettings.Username);
                h.Password(rabbitSettings.Password);
            });
    });
});

#endregion

builder.Services.Configure<RabbitMqSettings>(builder.Configuration.GetSection("RabbitMQ"));
builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

#endregion

#region DATABASE

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

#endregion

#region JWT CONFIGURATION

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();


var jwtSettings = builder.Configuration
    .GetSection("JwtSettings")
    .Get<JwtSettings>();

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings!.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.SecretKey))
        };
    });

builder.Services.AddAuthorization();

#endregion

#region USE CASES

builder.Services.AddScoped<LoginUseCase>();

builder.Services.AddScoped<CreateUserUseCase>();
builder.Services.AddScoped<GetUsersUseCase>();
builder.Services.AddScoped<GetUserByIdUseCase>();
builder.Services.AddScoped<UpdateUserUseCase>();
builder.Services.AddScoped<DeleteUserUseCase>();

#endregion

#region INFRASTRUCTURE

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IPasswordHashService, PasswordHashService>();

#endregion

#region CONTROLLERS

builder.Services.AddControllers();

#endregion

#region SWAGGER

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "CloudGames.Users API",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

#endregion

#region BUILD APP

var app = builder.Build();

#endregion

#region DATABASE SEED

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();

    await DatabaseSeeder.SeedAdminAsync(db, passwordHashService);
}

#endregion

#region MIDDLEWARES

app.UseMiddleware<ExceptionMiddleware>();
app.UseRequestLogging();

#endregion

#region SWAGGER UI

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudGames.Users API v1");
        c.RoutePrefix = string.Empty;
    });
}

#endregion

#region AUTH

app.UseAuthentication();
app.UseAuthorization();

#endregion

#region ENDPOINTS

app.MapControllers();

#endregion

app.Run();