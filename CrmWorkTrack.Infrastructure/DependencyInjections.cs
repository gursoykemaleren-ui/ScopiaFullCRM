using CrmWorkTrack.Application.Interfaces;
using CrmWorkTrack.Application.Interfaces.Auth;
using CrmWorkTrack.Application.Interfaces.Repositories;
using CrmWorkTrack.Infrastructure.Auth;
using CrmWorkTrack.Infrastructure.Persistence;
using CrmWorkTrack.Infrastructure.Repositories;
using CrmWorkTrack.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CrmWorkTrack.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

        services.AddScoped<IJobActivityService, JobActivityService>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(EfRepository<>));
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<IJobRepository, JobRepository>();
        services.AddScoped<IPermissionService, PermissionService>();

        return services;
    }
}
