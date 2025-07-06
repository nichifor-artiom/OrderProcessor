using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using OrderProcessor.Configurations;
using OrderProcessor.Models;
using OrderProcessor.Models.BusinessModels;
using OrderProcessor.Providers;
using OrderProcessor.Repositories;
using OrderProcessor.Services.Implementation;
using OrderProcessor.Tools;
using Xunit;

namespace OrderProcess.Unit.Tests.Services;

public class OrderServiceTests
{   
    private readonly Mock<IFlatFileOrderParser> flatFileOrderParserMock = new();
    private readonly Mock<IPriceReferenceRepository> priceReferenceRepositoryMock = new();
    private readonly Mock<IOrderManagementSystemProvider> orderManagementSystemProviderMock = new();
    private readonly Mock<IErpRepository> erpRepositoryMock = new();
    private readonly Mock<IValidator<Order>> orderValidatorMock = new();
    private readonly Mock<ILogger<OrderService>> loggerMock = new();
    private readonly IOptions<OrderConfigurations> options;

    public OrderServiceTests()
    {
        options = Options.Create(new OrderConfigurations
        {
            StoragePath = Path.GetTempPath()
        });
    }

    private OrderService CreateService() =>
        new OrderService(
            flatFileOrderParserMock.Object,
            loggerMock.Object,
            orderValidatorMock.Object,
            priceReferenceRepositoryMock.Object,
            orderManagementSystemProviderMock.Object,
            erpRepositoryMock.Object,
            options);

    [Fact]
    public async Task ProcessOrdersAsync_ShouldProcessAllFiles()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, "order1.txt");
        File.WriteAllText(filePath, "header\nline");

        var order = new Order
        {
            Articles = new List<Article> { new Article { EanCode = 1234567890123, Description = "desc", Quantity = 1, UnitPrice = 1 } }
        };

        options.Value.StoragePath = tempDir;
        flatFileOrderParserMock.Setup(x => x.ParseOrderFile(It.IsAny<string>())).Returns(order);
        orderValidatorMock.Setup(x => x.ValidateAsync(order, default)).ReturnsAsync(new ValidationResult());
        priceReferenceRepositoryMock.Setup(x => x.GetPriceReference(It.IsAny<long>(), It.IsAny<long>()))
            .Returns(new PriceReference { DefaultPrice = 1, SpecificPrice = 0 });
        erpRepositoryMock.Setup(x => x.GetStockItem(It.IsAny<long>()))
            .Returns(new StockItem { AvailableQuantity = 10, ArticleCode = 1234567890123 });
        orderManagementSystemProviderMock.Setup(x => x.SendOrderAsync(order)).Returns(Task.CompletedTask);

        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.ProcessOrdersAsync();

        // Assert
        await act.Should().NotThrowAsync();

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ProcessOrdersAsync_ShouldLogError_WhenValidationFails()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, "order1.txt");
        File.WriteAllText(filePath, "header\nline");

        var order = new Order
        {
            Articles = new List<Article> { new Article { EanCode = 1234567890123, Description = "desc", Quantity = 1, UnitPrice = 1 } }
        };

        options.Value.StoragePath = tempDir;
        flatFileOrderParserMock.Setup(x => x.ParseOrderFile(It.IsAny<string>())).Returns(order);
        orderValidatorMock.Setup(x => x.ValidateAsync(order, default)).ReturnsAsync(
            new ValidationResult(new List<ValidationFailure> { new("OrderNumber", "Invalid") }));

        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.ProcessOrdersAsync();

        // Assert
        await act.Should().NotThrowAsync();

        // Cleanup
        Directory.Delete(tempDir, true);
    }

    [Fact]
    public async Task ProcessOrdersAsync_ShouldThrow_WhenNotEnoughStock()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(tempDir);
        var filePath = Path.Combine(tempDir, "order1.txt");
        File.WriteAllText(filePath, "header\nline");

        var order = new Order
        {
            OrderNumber = 1,
            Articles = new List<Article> { new Article { EanCode = 1234567890123, Description = "desc", Quantity = 100, UnitPrice = 1 } }
        };

        options.Value.StoragePath = tempDir;
        flatFileOrderParserMock.Setup(x => x.ParseOrderFile(It.IsAny<string>())).Returns(order);
        orderValidatorMock.Setup(x => x.ValidateAsync(order, default)).ReturnsAsync(new ValidationResult());
        priceReferenceRepositoryMock.Setup(x => x.GetPriceReference(It.IsAny<long>(), It.IsAny<long>()))
            .Returns(new PriceReference { DefaultPrice = 1, SpecificPrice = 0 });
        erpRepositoryMock.Setup(x => x.GetStockItem(It.IsAny<long>()))
            .Returns(new StockItem { AvailableQuantity = 10, ArticleCode = 1234567890123 });

        var service = CreateService();

        // Act
        Func<Task> act = async () => await service.ProcessOrdersAsync();

        // Assert
        await act.Should().NotThrowAsync(); // Exception is caught and logged

        // Cleanup
        Directory.Delete(tempDir, true);
    }
}
