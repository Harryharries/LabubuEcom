using eCommerce.SharedLibrary.Middleware;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace eCommerce.SharedLibrary.DependencyInjection
{
    public static class SharedServiceContainer
    {
        public static IServiceCollection AddSharedService<TContext>
            (this IServiceCollection services, IConfiguration config, string fileName) where TContext: DbContext
        {
            services.AddDbContext<TContext>(option => option.UseSqlServer(config.GetConnectionString("eComm"),sqlserverOption => sqlserverOption.EnableRetryOnFailure()));

            Log.Logger = new LoggerConfiguration().MinimumLevel.Information().WriteTo.Debug().WriteTo.Console().WriteTo.File(
                path: $"{fileName}-.text", 
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information, 
                outputTemplate: "{TimeStamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {message:lj}{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day)
                .CreateLogger();

            JWTAuthenticationScheme.AddJWTAuthenticationScheme(services, config);
            return services;
        }

        public static IApplicationBuilder UseSharedPolicies(this IApplicationBuilder app)
        {
            app.UseMiddleware<GlobalException>();
            app.UseMiddleware<ListenToOnlyApiGateway>();
            return app;
        }
    }
}
