using Lumel.Application.Interfaces;
using Lumel.Infrastructure.Persistence;
using Lumel.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Lumel.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // Database context
        services.AddDbContext<LumelDbContext>(options =>
        {
            options.UseSqlServer(
                configuration.GetConnectionString("DbConnection"),
                sqlOptions =>
                {
                    sqlOptions.EnableRetryOnFailure(3);
                    sqlOptions.CommandTimeout(120);
                });
        });

        // Services
        services.AddScoped<IAnalyticsService, AnalyticsService>();
        services.AddScoped<ICsvImportService, CsvImportService>();
        services.AddScoped<IDataRefreshService, DataRefreshService>();

        // Background service for scheduled refresh
        services.AddHostedService<DataRefreshBackgroundService>();

        return services;
    }
}
