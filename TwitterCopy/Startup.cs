using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;
using TwitterCopy.Areas.Identity;
using TwitterCopy.Areas.Identity.Services;
using TwitterCopy.Data;

namespace TwitterCopy
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            services.AddDbContext<TwitterCopyContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("TwitterCopyContextConnection")));

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

            services.AddMvc()
                .AddRazorPagesOptions(options =>
                {
                    options.AllowAreas = true;
                    options.Conventions.AuthorizeAreaFolder("Identity", "/Account/Manage");
                    options.Conventions.AuthorizeAreaPage("Identity", "/Account/Logout");
                    // Removes /Identity/ from the links
                    options.Conventions.AddAreaFolderRouteModelConvention("Identity", "/Account/",
                        model =>
                        {
                            foreach (var selector in model.Selectors)
                            {
                                var attributeRouteModel = selector.AttributeRouteModel;
                                attributeRouteModel.Order = -1;
                                attributeRouteModel.Template = attributeRouteModel.Template.Remove(0, "Identity".Length);
                            }
                        });
                    options.Conventions.AddAreaPageRoute("Identity", "/Account/Register", "/Account/Signup");
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            services.AddRouting(options => options.LowercaseUrls = true);

            services.ConfigureApplicationCookie(options =>
            {
                options.Cookie.Name = "TwitterCopyCookies";
                options.ExpireTimeSpan = TimeSpan.FromDays(10);

                // /Identity/ removed from links
                options.LoginPath = $"/Account/Login";
                options.LogoutPath = $"/Account/Logout";
                options.AccessDeniedPath = $"/Account/AccessDenied";
            });

            services.AddSingleton<IEmailSender, EmailSender>();

            services.Configure<AuthMessageSenderOptions>(Configuration);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();
            app.UseCookiePolicy();

            app.UseAuthentication();

            // Solution for redirecting from https://github.com/aspnet/Identity/issues/1815
            //app.Use(async (context, next) =>
            //{
            //    var request = context.Request;
            //    if (request.Path == "/Identity/Account/Login")
            //    {
            //        context.Response.Redirect("/Account/Login");
            //    }
            //    else
            //    {
            //        await next.Invoke();
            //    }
            //});

            app.UseMvc();
        }
    }
}
