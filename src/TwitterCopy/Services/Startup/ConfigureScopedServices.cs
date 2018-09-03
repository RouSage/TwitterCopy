using Microsoft.Extensions.DependencyInjection;
using TwitterCopy.Core.Interfaces;
using TwitterCopy.Core.Services;
using TwitterCopy.Infrastructure.Data;

namespace TwitterCopy.Services.Startup
{
    public static partial class ConfigurationExtensions
    {
        public static IServiceCollection ConfigureScopedServices(this IServiceCollection services)
        {
            services.AddScoped(typeof(IRepository<>), typeof(EfRepository<>));
            services.AddScoped<ITweetRepository, TweetRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<ITweetService, TweetService>();

            return services;
        }
    }
}
