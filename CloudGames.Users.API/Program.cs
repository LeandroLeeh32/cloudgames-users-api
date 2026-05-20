using CloudGames.Users.API.Logging;
using CloudGames.Users.API.Security;
using CloudGames.Users.Application.Interfaces.Messaging;
using CloudGames.Users.Application.Interfaces.Security;
using CloudGames.Users.Infrastructure.Messaging.Configuration;
using CloudGames.Users.Infrastructure.Messaging.EventBus;
using MassTransit;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Options;
using MongoDB.Driver;
using Prometheus;
using Serilog;
using System.Text;
using Users.API.Middlewares;
using Users.Application.Interfaces.Repositories;
using Users.Application.Security;
using Users.Application.UseCases.Auth;
using Users.Application.UseCases.Users;
using Users.Infrastructure.Persistence.Context;
using Users.Infrastructure.Persistence.Mappings;
using Users.Infrastructure.Repositories;
using Users.Infrastructure.Security;
using Users.Infrastructure.Seed;
using CloudGames.Notifications.Application.IntegrationEvents.Users;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseCloudGamesLogging("users-api");

    Log.Information("Starting CloudGames.Users API...");

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

            var rabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "rabbitmq-service";

            var rabbitVirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VIRTUAL_HOST")
                                     ?? settings.VirtualHost
                                     ?? "/";

            var rabbitUsername = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME")
                                 ?? settings.Username
                                 ?? throw new InvalidOperationException("RABBITMQ_USERNAME não configurado.");

            var rabbitPassword = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD")
                                 ?? settings.Password
                                 ?? throw new InvalidOperationException("RABBITMQ_PASSWORD não configurado.");

            Log.Information("RabbitMQ Host: {RabbitHost}", rabbitHost);

            cfg.Host(rabbitHost, rabbitVirtualHost, h =>
            {
                h.Username(rabbitUsername);
                h.Password(rabbitPassword);
            });

            cfg.Message<UserCreatedIntegrationEvent>(x => x.SetEntityName("UserCreatedIntegrationEvent"));
            cfg.ConfigureEndpoints(context);
        });
    });

    builder.Services.AddScoped<IEventPublisher, MassTransitEventPublisher>();

    #endregion

    #region DATABASE

    UserClassMap.Register();

    var mongoConnection = Environment.GetEnvironmentVariable("MongoSettings__ConnectionString")
                          ?? builder.Configuration["MongoSettings:ConnectionString"]
                          ?? throw new InvalidOperationException("MongoSettings:ConnectionString não configurado.");

    var mongoDatabase = Environment.GetEnvironmentVariable("MongoSettings__Database")
                        ?? builder.Configuration["MongoSettings:Database"]
                        ?? throw new InvalidOperationException("MongoSettings:Database não configurado.");

    var mongoSettings = new MongoSettings
    {
        ConnectionString = mongoConnection,
        Database = mongoDatabase
    };

    builder.Services.AddSingleton(mongoSettings);
    builder.Services.AddSingleton<IMongoClient>(_ => new MongoClient(mongoConnection));
    builder.Services.AddScoped<MongoContext>();

    #endregion

    #region JWT

    builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();

    var jwtIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER")
                    ?? builder.Configuration["JwtSettings:Issuer"]
                    ?? throw new InvalidOperationException("JWT_ISSUER não configurado.");

    var jwtAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE")
                      ?? builder.Configuration["JwtSettings:Audience"]
                      ?? throw new InvalidOperationException("JWT_AUDIENCE não configurado.");

    var jwtSecret = Environment.GetEnvironmentVariable("JWT_SECRET")
                    ?? builder.Configuration["JwtSettings:SecretKey"]
                    ?? throw new InvalidOperationException("JWT_SECRET não configurado.");

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
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret))
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

    var app = builder.Build();

    #region DATABASE SEED

    using (var scope = app.Services.CreateScope())
    {
        var context = scope.ServiceProvider.GetRequiredService<MongoContext>();
        var passwordHashService = scope.ServiceProvider.GetRequiredService<IPasswordHashService>();

        await DatabaseSeeder.SeedAdminAsync(context, passwordHashService);
    }

    #endregion

    #region MIDDLEWARES

    app.UseMiddleware<ExceptionMiddleware>();
    app.UseRequestLogging();
    app.UseHttpMetrics();

    #endregion

    #region SWAGGER

    app.UseSwagger(c =>
    {
        c.PreSerializeFilters.Add((swagger, httpReq) =>
        {
            swagger.Servers = new List<OpenApiServer>
            {
                new() { Url = $"{httpReq.Scheme}://{httpReq.Host}/users" }
            };
        });
    });

    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("swagger/v1/swagger.json", "CloudGames.Users API v1");
        c.RoutePrefix = string.Empty;
    });

    #endregion

    #region AUTH

    app.UseAuthentication();
    app.UseAuthorization();

    #endregion

    #region ENDPOINTS

    app.MapControllers();
    app.MapMetrics();

    #endregion

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application stopped due to exception");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
