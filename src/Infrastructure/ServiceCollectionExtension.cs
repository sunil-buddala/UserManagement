﻿using System;
using Application.Common.Interfaces;
using Domain.Entities;
using Infrastructure.Persistence;
using Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure
{
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
                services.AddDbContext<ApplicationDbContext>(options => options.UseInMemoryDatabase("TestDb"));
            else
            {
                services.AddDbContext<ApplicationDbContext>(options =>
                {
                    options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"),
                        builder => builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName));
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
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            services.AddScoped<ITokenService, TokenService>();
        }
    }
}