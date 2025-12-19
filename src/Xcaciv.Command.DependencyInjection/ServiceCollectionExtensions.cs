using System;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Xcaciv.Command;
using Xcaciv.Command.FileLoader;
using Xcaciv.Command.Interface;

namespace Xcaciv.Command.DependencyInjection;

/// <summary>
/// Extension methods for registering Xcaciv.Command services with dependency injection containers.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddXcacivCommand(this IServiceCollection services)
    {
        return services.AddXcacivCommand(_ => { });
    }

    public static IServiceCollection AddXcacivCommand(
        this IServiceCollection services,
        Action<CommandControllerOptions> configure)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configure == null) throw new ArgumentNullException(nameof(configure));

        services.Configure(configure);

        services.TryAddSingleton<ICommandRegistry, CommandRegistry>();
        services.TryAddSingleton<ICommandFactory, CommandFactory>();
        services.TryAddSingleton<ICommandExecutor, CommandExecutor>();
        services.TryAddSingleton<IPipelineExecutor, PipelineExecutor>();
        services.TryAddSingleton<ICommandLoader, CommandLoader>();
        services.TryAddSingleton<ICrawler, Crawler>();
        services.TryAddSingleton<IVerifiedSourceDirectories>(sp =>
            new VerifiedSourceDirectories(new System.IO.Abstractions.FileSystem()));

        services.TryAddSingleton<IAuditLogger, NoOpAuditLogger>();
        services.TryAddSingleton<IOutputEncoder, NoOpEncoder>();
        services.TryAddSingleton<IHelpService, HelpService>();

        services.TryAddSingleton<ICommandController, CommandController>();

        return services;
    }

    public static IServiceCollection AddXcacivCommand(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        if (services == null) throw new ArgumentNullException(nameof(services));
        if (configuration == null) throw new ArgumentNullException(nameof(configuration));

        services.Configure<CommandControllerOptions>(
            configuration.GetSection(CommandControllerOptions.SectionName));

        services.Configure<PipelineOptions>(
            configuration.GetSection(PipelineOptions.SectionName));

        return services.AddXcacivCommand(_ => { });
    }

    public static IServiceCollection WithAuditLogger<T>(this IServiceCollection services)
        where T : class, IAuditLogger
    {
        services.AddSingleton<IAuditLogger, T>();
        return services;
    }

    public static IServiceCollection WithStructuredAuditLogging(this IServiceCollection services)
    {
        services.AddSingleton<IAuditLogger, StructuredAuditLogger>();
        return services;
    }

    public static IServiceCollection WithOutputEncoder<T>(this IServiceCollection services)
        where T : class, IOutputEncoder
    {
        services.AddSingleton<IOutputEncoder, T>();
        return services;
    }

    public static IServiceCollection ConfigurePipeline(
        this IServiceCollection services,
        Action<PipelineOptions> configure)
    {
        services.Configure(configure);
        return services;
    }
}
