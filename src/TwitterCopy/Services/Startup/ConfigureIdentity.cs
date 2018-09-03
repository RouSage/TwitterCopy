using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System;
using TwitterCopy.Core.Entities;
using TwitterCopy.Infrastructure.Data;

namespace TwitterCopy.Services.Startup
{
    public static partial class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<TwitterCopyUser, TwitterCopyRole>(options =>
            {
                // Password setting
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 6;
                options.Password.RequiredUniqueChars = 1;
                options.Password.RequireLowercase = false;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;

                // Lockout settings
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(20);
                options.Lockout.MaxFailedAccessAttempts = 10;
                options.Lockout.AllowedForNewUsers = true;

                // Signin setting
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedPhoneNumber = false;

                // User setting
                options.User.RequireUniqueEmail = true;
            })
                .AddEntityFrameworkStores<TwitterCopyContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
