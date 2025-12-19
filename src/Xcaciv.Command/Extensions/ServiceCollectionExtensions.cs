using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command.Extensions;

/// <summary>
/// Extension methods for registering Xcaciv.Command services with dependency injection containers.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Registers all Xcaciv.Command services with the DI container using default configuration.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddXcacivCommand(this IServiceCollection services)
    {
        return services.AddXcacivCommand(_ => { });
    }

    /// <summary>
    /// Registers all Xcaciv.Command services with the DI container using custom configuration.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configure">Action to configure CommandControllerOptions.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddXcacivCommand(
        this IServiceCollection services,
        Action<CommandControllerOptions> configure)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        // Register options
        services.Configure(configure);

        // Register core services as singletons (typical for command framework)
        services.TryAddSingleton<ICommandRegistry, CommandRegistry>();
        services.TryAddSingleton<ICommandFactory, CommandFactory>();
        services.TryAddSingleton<ICommandExecutor, CommandExecutor>();
        services.TryAddSingleton<IPipelineExecutor, PipelineExecutor>();
        services.TryAddSingleton<ICommandLoader, CommandLoader>();
        services.TryAddSingleton<ICrawler, Crawler>();
        services.TryAddSingleton<IVerifiedSourceDirectories>(sp => 
            new VerifiedSourceDirectories(new System.IO.Abstractions.FileSystem()));

        // Register audit logger (default to NoOp, can be overridden)
        services.TryAddSingleton<IAuditLogger, NoOpAuditLogger>();

        // Register output encoder (default to NoOp, can be overridden)
        services.TryAddSingleton<IOutputEncoder, NoOpEncoder>();

        // Register the controller
        services.TryAddSingleton<ICommandController, CommandController>();

        return services;
    }

    /// <summary>
    /// Registers all Xcaciv.Command services with the DI container using IConfiguration.
    /// </summary>
    /// <param name="services">The service collection to register services with.</param>
    /// <param name="configuration">The configuration section containing command options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection AddXcacivCommand(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        // Bind options from configuration
        services.Configure<CommandControllerOptions>(
            configuration.GetSection(CommandControllerOptions.SectionName));

        services.Configure<PipelineOptions>(
            configuration.GetSection(PipelineOptions.SectionName));

        // Register core services
        return services.AddXcacivCommand(_ => { });
    }

    /// <summary>
    /// Replaces the default IAuditLogger with a custom implementation.
    /// </summary>
    /// <typeparam name="T">The custom audit logger type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection WithAuditLogger<T>(this IServiceCollection services)
        where T : class, IAuditLogger
    {
        services.AddSingleton<IAuditLogger, T>();
        return services;
    }

    /// <summary>
    /// Configures the command controller to use structured audit logging.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection WithStructuredAuditLogging(this IServiceCollection services)
    {
        services.AddSingleton<IAuditLogger, StructuredAuditLogger>();
        return services;
    }

    /// <summary>
    /// Replaces the default IOutputEncoder with a custom implementation.
    /// </summary>
    /// <typeparam name="T">The custom encoder type.</typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection WithOutputEncoder<T>(this IServiceCollection services)
        where T : class, IOutputEncoder
    {
        services.AddSingleton<IOutputEncoder, T>();
        return services;
    }

    /// <summary>
    /// Configures pipeline options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configure">Action to configure pipeline options.</param>
    /// <returns>The service collection for method chaining.</returns>
    public static IServiceCollection ConfigurePipeline(
        this IServiceCollection services,
        Action<PipelineOptions> configure)
    {
        services.Configure(configure);
        return services;
    }
}
