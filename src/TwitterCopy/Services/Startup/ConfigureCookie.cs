using Microsoft.Extensions.DependencyInjection;
using System;

namespace TwitterCopy.Services.Startup
{
    public static partial class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureCookie(this IServiceCollection services)
        {
            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "TwitterCopyCookies";
                options.ExpireTimeSpan = TimeSpan.FromDays(30);

                options.LoginPath = $"/Account/Login";
                options.LogoutPath = $"/Account/Logout";
                options.AccessDeniedPath = $"/Account/AccessDenied";
            });

            return services;
        }
    }
}
