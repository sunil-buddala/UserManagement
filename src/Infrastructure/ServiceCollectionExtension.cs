using System;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure;

/// <summary>
/// Extension Class For <see cref="IServiceCollection"/> Interface
/// </summary>
public static class ServiceCollectionExtension
{
    /// <summary>
    /// Injects Infrastructure Dependencies Into Dependency Injection Container
    /// </summary>
    /// <param name="services"><see cref="IServiceCollection"/> Interface</param>
    /// <param name="configuration"><see cref="IConfiguration"/> Interface</param>
    public static void AddInfrastructure(this IServiceCollection services,IConfiguration configuration)
    {
        if (Convert.ToBoolean(configuration.GetValue<bool>("UseInMemoryDatabase")))
        {
            services.AddDbContext<UsersDbReadOnlyContext>(options => options.UseInMemoryDatabase("TestDb"));
            services.AddDbContext<UsersDbWriteContext>(options => options.UseInMemoryDatabase("TestDb"));
        }
        else
        {
            services.AddDbContext<UsersDbReadOnlyContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("UsersDbReadOnlyConnection");
                options.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString));
            });
            services.AddDbContext<UsersDbWriteContext>(options =>
            {
                var connectionString = configuration.GetConnectionString("UsersDbWriteConnection");
                options.UseMySql(connectionString,
                    ServerVersion.AutoDetect(connectionString));
            });
        }

        services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequiredLength = 4;
                options.Password.RequireDigit = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<UsersDbReadOnlyContext>()
            .AddEntityFrameworkStores<UsersDbWriteContext>()
            .AddDefaultTokenProviders();

        services.AddScoped<IUsersDbReadOnlyContext>(x => x.GetService<UsersDbReadOnlyContext>()!);
        services.AddScoped<IUsersDbWriteContext>(x => x.GetService<UsersDbWriteContext>()!);
        services.AddScoped<ITokenGenerator, TokenGenerator>();
        services.AddScoped<IAccessTokenService, AccessTokenService>();
        services.AddScoped<IRefreshTokenService, RefreshTokenService>();
        services.AddScoped<IRefreshTokenValidator, RefreshTokenValidator>();
        services.AddScoped<IAuthenticateService, AuthenticateService>();
    }
}