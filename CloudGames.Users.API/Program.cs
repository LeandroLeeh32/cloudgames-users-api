using CloudGames.Users.API.Security;
using CloudGames.Users.Application.Interfaces.Messaging;
using CloudGames.Users.Application.Interfaces.Security;
using CloudGames.Users.Infrastructure.Messaging.Configuration;
using CloudGames.Users.Infrastructure.Messaging.EventBus;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
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

try
{
    logger.Info("Starting CloudGames.Users API...");

    #region BUILDER

    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Host.UseNLog();

    #endregion

    #region CONFIGURATION

    builder.Services.Configure<RabbitMqSettings>(
        builder.Configuration.GetSection("RabbitMQ"));

    builder.Services.Configure<JwtSettings>(
        builder.Configuration.GetSection("JwtSettings"));

    #endregion

    #region MASSTRANSIT

    builder.Services.AddMassTransit(x =>
    {
        x.AddConsumers(typeof(Program).Assembly);

        x.UsingRabbitMq((context, cfg) =>
        {
            var settings = context.GetRequiredService<IOptions<RabbitMqSettings>>().Value;

            //LOCAL + DOCKER
            var isDocker = Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true";

            var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST")
                             ?? (isDocker ? settings.Host : "localhost");

            logger.Info($"RabbitMQ Host: {rabbitHost}");

            cfg.Host(rabbitHost, settings.VirtualHost, h =>
            {
                h.Username(settings.Username);
                h.Password(settings.Password);
            });

            cfg.ConfigureEndpoints(context);
        });
    });

    builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

    #endregion

    #region DATABASE

    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

    if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
    {
        connectionString = "Data Source=/app/Data/database.db";
    }

    builder.Services.AddDbContext<AppDbContext>(options =>
        options.UseSqlite(connectionString));

    #endregion

    #region JWT

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

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(Policies.AdminActive, policy =>
            policy.RequireRole("Admin")
                  .RequireClaim("isActive", "true"));
    });

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

    #region BUILD

    var app = builder.Build();

    #endregion

    #region DATABASE SEED

    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();

        await db.Database.MigrateAsync();
        await DatabaseSeeder.SeedAdminAsync(db, passwordHashService);
    }

    #endregion

    #region MIDDLEWARES

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseRequestLogging();

    #endregion

    #region SWAGGER

    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "CloudGames.Users API v1");
        c.RoutePrefix = string.Empty;
    });

    #endregion

    #region AUTH

    app.UseAuthentication();
    app.UseAuthorization();

    #endregion

    #region ENDPOINTS

    app.MapControllers();

    #endregion

    app.Run();
}
catch (Exception ex)
{
    logger.Error(ex, "Application stopped due to exception");
    throw;
}
finally
{
    LogManager.Shutdown();
}