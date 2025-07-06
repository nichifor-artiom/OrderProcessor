using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using OrderProcessor.Configurations;
using OrderProcessor.Extensions;
using OrderProcessor.Models;
using OrderProcessor.Providers;
using OrderProcessor.Providers.Implementation;
using OrderProcessor.Repositories;
using OrderProcessor.Repositories.Implementation;
using OrderProcessor.Services;
using OrderProcessor.Services.Implementation;
using OrderProcessor.Tools;
using OrderProcessor.Tools.Implementation;

namespace OrderProcessor;

public static class ServiceBindings
{
    public static IServiceProvider BuildServiceProvider(Action<IServiceCollection> serviceOverrides = null)
    {
        var services = new ServiceCollection();
        var config = BuildConfigurationProvider();
        services.Configure<OrderConfigurations>(config.GetSection("Order"));

        // utilities
        services.AddOptions();
        services.AddPolicyRegistry();

        // logging
        services.AddLogging(builder => builder
            .ClearProviders()
            .AddConfiguration(config.GetSection("Logging")));

        // Services, providers, repositories and clients
        services.TryAddScoped<IOrderService, OrderService>();
        services.TryAddScoped<IFlatFileOrderParser, FlatFileOrderParser>();
        services.TryAddSingleton<IPriceReferenceRepository, PriceReferenceRepository>();
        services.TryAddSingleton<IErpRepository, ErpRepository>();
        services.AddHttpClient<IOrderManagementSystemProvider, OrderManagementSystemProvider>(
                    c =>
                    {
                        c.BaseAddress = new Uri(config["OrderManagementSystem:BaseUrl"]);
                    })
            .ConfigureHttpClientCommon(new[] { 3, 7, 12 });

        // validators
        services.TryAddSingleton<IValidator<Order>, OrderValidator>();
        services.TryAddSingleton<IValidator<Article>, ArticleValidator>();

        serviceOverrides?.Invoke(services);
        return services.BuildServiceProvider();
    }

    private static IConfiguration BuildConfigurationProvider()
    {
        var configBuilder = new ConfigurationBuilder();
        configBuilder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

        return configBuilder.Build();
    }
}
