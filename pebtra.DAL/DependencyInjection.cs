using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Pebtra.DAL.Repositories;

namespace Pebtra.DAL
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddDataAccess(this IServiceCollection services, IConfiguration configuration)
        {
            // Register DbContext
            services.AddDbContext<FinContext>(options =>
                options.UseSqlServer(
                    configuration.GetConnectionString("DefaultConnection"),
                    b => b.MigrationsAssembly("pebtra.DAL")));

            // Register repositories
            services.AddScoped<ITransactionRepository, TransactionRepository>();

            return services;
        }
    }
} 