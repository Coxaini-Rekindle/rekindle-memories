﻿using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Rekindle.Memories.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddMediatR(cfg => { cfg.RegisterServicesFromAssembly(typeof(DependencyInjection).Assembly); });
        
        return services;
    }
}

public interface IApplicationAssemblyMarker
{
}