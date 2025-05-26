using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rebus.Serialization.Json;
using Rekindle.Memories.Application.Common.Messaging;
using Rekindle.Memories.Contracts;

namespace Rekindle.Memories.Infrastructure.Messaging;

/// <summary>
/// Configuration for Rebus message bus
/// </summary>
public static class RebusConfig
{
    /// <summary>
    /// Registers Rebus with RabbitMQ transport in the service collection
    /// </summary>
    /// <param name="services">The service collection</param>
    /// <param name="configuration">The configuration</param>
    /// <returns>The service collection with Rebus registered</returns>
    public static IServiceCollection AddRebusMessageBus(this IServiceCollection services, IConfiguration configuration)
    {
        var rabbitMqConnectionString = configuration.GetConnectionString("RabbitMQ")
                                       ?? "amqp://guest:guest@localhost:5672";

        var serviceName = configuration["ServiceName"] ?? "Rekindle.UserGroups";

        services.AddRebus(configure => configure
            .Transport(t => t.UseRabbitMq(rabbitMqConnectionString, serviceName))
            .Routing(r =>
            {
                r.TypeBased()
                    .MapAssemblyOf<IEvent>(serviceName);
            })
            .Serialization(s => s.UseNewtonsoftJson())
            .Logging(l => l.ColoredConsole())
        );

        services.AddTransient<IEventPublisher, RebusEventPublisher>();

        return services;
    }

    /// <summary>
    /// Starts the Rebus message bus
    /// </summary>
    /// <param name="serviceProvider">The service provider</param>
    public static void StartRebus(this IServiceProvider serviceProvider)
    {
        // Start the bus
        serviceProvider.GetRequiredService<Rebus.Bus.IBus>();
    }
}

public interface IInfrastructureAssemblyMarker
{
}