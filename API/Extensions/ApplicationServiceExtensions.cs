using API.Data;
using API.Helpers;
using API.Interfaces;
using API.Services;
using API.SignalR;
using Microsoft.EntityFrameworkCore;

namespace API.Extensions
{
    public static class ApplicationServiceExtensions
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration config) 
        {
            services.AddCors();
            services.AddScoped<ITokenService, TokenService>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());
            services.Configure<CloudinarySettings>(config.GetSection("CloudinarySettings"));
            services.AddScoped<IPhotoService, PhotoService>();
            services.AddScoped<LogUserActivity>(); //add log user activity as a service
            services.AddSignalR(); //add signalR service
            services.AddSingleton<PresenceTracker>(); //add presence tracker as a singleton service
            services.AddScoped<IUnitOfWork, UnitOfWork>(); //add unit of work as a scoped service

            return services;
        }
    }
}