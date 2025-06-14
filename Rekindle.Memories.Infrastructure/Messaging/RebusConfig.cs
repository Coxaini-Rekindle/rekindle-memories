using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Rebus.Config;
using Rebus.Routing.TypeBased;
using Rekindle.Memories.Application;
using Rekindle.Memories.Application.Common.Messaging;
using Rekindle.Search.Contracts;
using Rekindle.UserGroups.Contracts.GroupEvents;
using Rekindle.UserGroups.Contracts.UserEvents;
using IEvent = Rekindle.UserGroups.Contracts.IEvent;

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

        var serviceName = configuration["ServiceName"];

        services.AddRebus(configure => configure
                .Transport(t => t.UseRabbitMq(rabbitMqConnectionString, serviceName))
                .Logging(l => l.ColoredConsole()),
            onCreated: async bus =>
            {
                await bus.Subscribe<GroupCreatedEvent>();
                await bus.Subscribe<GroupUpdatedEvent>();
                await bus.Subscribe<GroupDeletedEvent>();
                await bus.Subscribe<UserJoinedGroupEvent>();
                await bus.Subscribe<UserLeftGroupEvent>();
                await bus.Subscribe<UserNameChangedEvent>();
                await bus.Subscribe<UserAvatarChangedEvent>();
                await bus.Subscribe<ImageFacesAnalyzedEvent>();
            }
        );
        services.AutoRegisterHandlersFromAssemblyOf<IApplicationAssemblyMarker>();
        services.AddTransient<IEventPublisher, RebusEventPublisher>();

        return services;
    }
}

public interface IInfrastructureAssemblyMarker
{
}