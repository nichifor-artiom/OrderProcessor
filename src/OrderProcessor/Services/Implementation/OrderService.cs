using FluentValidation;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OrderProcessor.Configurations;
using OrderProcessor.Models;
using OrderProcessor.Models.BusinessModels;
using OrderProcessor.Providers;
using OrderProcessor.Repositories;
using OrderProcessor.Tools;

namespace OrderProcessor.Services.Implementation;

public class OrderService : IOrderService
{
    private readonly IFlatFileOrderParser flatFileOrderParser;
    private readonly IPriceReferenceRepository priceReferenceRepository;
    private readonly IOrderManagementSystemProvider orderManagementSystemProvider;
    private readonly IErpRepository erpRepository;
    private readonly OrderConfigurations config;
    private readonly IValidator<Order> validator;
    private readonly ILogger<OrderService> logger;

    public OrderService(IFlatFileOrderParser flatFileOrderParser,
        ILogger<OrderService> logger,
        IValidator<Order> validator,
        IPriceReferenceRepository priceReferenceRepository,
        IOrderManagementSystemProvider orderManagementSystemProvider,
        IErpRepository erpRepository,
        IOptions<OrderConfigurations> options)
    {
        this.flatFileOrderParser = flatFileOrderParser;
        this.logger = logger;
        this.validator = validator;
        this.priceReferenceRepository = priceReferenceRepository;
        this.orderManagementSystemProvider = orderManagementSystemProvider;
        this.erpRepository = erpRepository;
        this.config = options.Value;
    }

    public async Task ProcessOrdersAsync()
    {
        this.logger.LogInformation("Starting order processing");

        // TODO - use a storage (blob?) and implement an retrieval mechanism
        var folderPath = Path.GetFullPath(this.config.StoragePath);
        var files = Directory.GetFiles(folderPath);

        foreach (var file in files)
        {
            try
            {
                await this.ProcessOrderAsync(file);
            }
            catch (Exception ex)
            {
                this.logger.LogError(ex, $"Failed to process file: {Path.GetFileName(file)}");
            }
        }

        this.logger.LogInformation("Finishing order processing");
    }

    private async Task ProcessOrderAsync(string file)
    {
        var order = this.flatFileOrderParser.ParseOrderFile(file);

        await this.ValidateOrder(Path.GetFileName(file), order);
        this.ValidatePriceReference(Path.GetFileName(file), order);
        this.ProcessErpCheck(order);

        await this.orderManagementSystemProvider.SendOrderAsync(order);
    }

    private void ProcessErpCheck(Order order)
    {
        var stockItemsToUpdate = new List<StockItem>();

        foreach (var article in order.Articles)
        {
            var stockItem = this.erpRepository.GetStockItem(article.EanCode);

            var quantityLeft = stockItem.AvailableQuantity - article.Quantity;

            if (quantityLeft < 0)
            {
                this.SendNotification(
                    $"Order {order.OrderNumber} is being cancelled because there are not enough articles: {article.EanCode}");
                throw new Exception($"Not enough articles: {article.EanCode}");
            }

            stockItem.AvailableQuantity = quantityLeft;
            stockItemsToUpdate.Add(stockItem);
        }

        foreach (var stockItem in stockItemsToUpdate)
        {
            this.erpRepository.UpsertStockItem(stockItem);
        }
    }

    private async Task ValidateOrder(string fileName, Order order)
    {
        var validationResult = await this.validator.ValidateAsync(order);

        if (!validationResult.IsValid)
        {
            var errors = string.Join("; ", validationResult.Errors.Select(e => e.ErrorMessage));
            throw new ValidationException($"Validation failed for file {fileName}: {errors}");
        }
    }

    private void ValidatePriceReference(string fileName, Order order)
    {
        foreach(var article in order.Articles)
        {
            var validationResult = this.priceReferenceRepository.GetPriceReference(article.EanCode, order.EanCodeOfBuyer);

            if (validationResult == null)
            {
                throw new ValidationException($"Price reference not found for article {article.EanCode} in file {fileName}");
            }

            if (validationResult.SpecificPrice > 0 && Math.Abs(article.UnitPrice - validationResult.SpecificPrice) > 0.01m)
            {
                article.UnitPrice = validationResult.SpecificPrice;
                this.SendNotification(
                    $"Unit price for order: {order.OrderNumber}, article: {article.EanCode} changed to specific price: {validationResult.SpecificPrice}");
                continue;
            }

            if (validationResult.DefaultPrice > 0 && Math.Abs(article.UnitPrice - validationResult.DefaultPrice) > 0.01m)
            {
                article.UnitPrice = validationResult.DefaultPrice;
                this.SendNotification(
                    $"Unit price for order: {order.OrderNumber}, article: {article.EanCode} changed to default price: {validationResult.DefaultPrice}");
            }
        }
    }

    private void SendNotification(string notificationMessage)
    {
        // TODO - implement a notification service to send notifications to the user (email with and smtp client?)
        this.logger.LogInformation(notificationMessage);
    }
}
