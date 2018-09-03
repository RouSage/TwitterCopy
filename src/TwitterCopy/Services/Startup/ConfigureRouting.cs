using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace TwitterCopy.Services.Startup
{
    public static partial class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureRouting(this IServiceCollection services)
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
                    options.Conventions.AddPageRoute("/Profiles/Likes", "{slug}/Likes");
                })
                .AddJsonOptions(options =>
                {
                    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
                })
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_1);

            return services;
        }
    }
}
