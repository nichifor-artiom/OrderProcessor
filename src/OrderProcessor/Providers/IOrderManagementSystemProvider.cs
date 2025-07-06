using OrderProcessor.Models;

namespace OrderProcessor.Providers;

public interface IOrderManagementSystemProvider
{
    Task SendOrderAsync(Order order);
}
