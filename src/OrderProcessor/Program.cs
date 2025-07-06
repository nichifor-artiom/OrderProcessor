using Microsoft.Extensions.DependencyInjection;
using OrderProcessor.Services;

namespace OrderProcessor;

class Program
{
    static async Task Main(string[] args)
    {
        // Create and run the host
        var serviceProvider = ServiceBindings.BuildServiceProvider();
        using var scope = serviceProvider.CreateScope();

        var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

        try
        {
            await orderService.ProcessOrdersAsync();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An error occurred while processing the orders: {ex.Message}");
        }
    }
}
