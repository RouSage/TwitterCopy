using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using System;
using TwitterCopy.Data;
using TwitterCopy.Entities;

namespace TwitterCopy.Services
{
    public static class CustomServices
    {
        public static IServiceCollection AddCustomizedMvc(this IServiceCollection services)
        {
            services.AddMvc(options => options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute()))
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AddPageRoute("/Account/Register", "/Account/Signup");
                    options.Conventions.AddPageRoute("/Profiles/Index", "{slug}");
                    options.Conventions.AddPageRoute("/Account/Settings/Index", "Account/Settings/Account");
                    options.Conventions.AddPageRoute("/Account/Settings/ChangePassword", "Account/Settings/Password");
                    options.Conventions.AddPageRoute("/Profiles/Following", "{slug}/Following");
                    options.Conventions.AddPageRoute("/Profiles/Followers", "{slug}/Followers");
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            return services;
        }

        public static IServiceCollection AddCustomizedIdentity(this IServiceCollection services)
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

        public static IServiceCollection CustomizedApplicationCookie(this IServiceCollection services)
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
