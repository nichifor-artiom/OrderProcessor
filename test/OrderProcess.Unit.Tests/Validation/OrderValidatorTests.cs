using FluentValidation.TestHelper;
using OrderProcessor.Models;
using Xunit;

namespace OrderProcess.Unit.Tests.Validation
{
    public class OrderValidatorTests
    {
        private readonly OrderValidator validator = new();

        private Order GetValidOrder() => new Order
        {
            FileTypeIdentifier = "ORD",
            OrderNumber = 12345,
            OrderDate = DateTime.UtcNow,
            EanCodeOfBuyer = 1234567890123,
            EanCodeOfSupplier = 9876543210123,
            Comment = "Test",
            Articles = new List<Article>
            {
                new Article
                {
                    EanCode = 1234567890123,
                    Description = "Test Article",
                    Quantity = 1,
                    UnitPrice = 10.0m
                }
            }
        };

        [Fact]
        public void ValidOrder_ShouldPass()
        {
            // Arrange, Act
            var result = validator.TestValidate(GetValidOrder());

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Theory]
        [InlineData("XXX")]
        [InlineData("")]
        public void InvalidTypeIdentifier_ShouldFail(string fileTypeIdentifier)
        {
            // Arrange
            var order = GetValidOrder();
            order.FileTypeIdentifier = fileTypeIdentifier;

            // Act
            var result = validator.TestValidate(order);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.FileTypeIdentifier);
        }

        [Fact]
        public void OrderNumber_LessThanOrEqualToZero_ShouldFail()
        {
            // Arrange
            var order = GetValidOrder();
            order.OrderNumber = 0;

            // Act
            var result = validator.TestValidate(order);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.OrderNumber);
        }

        [Fact]
        public void EanCodeOfBuyer_InvalidLength_ShouldFail()
        {
            // Arrange
            var order = GetValidOrder();
            order.EanCodeOfBuyer = 123;

            // Act
            var result = validator.TestValidate(order);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EanCodeOfBuyer);
        }

        [Fact]
        public void EanCodeOfSupplier_InvalidLength_ShouldFail()
        {
            // Arrange
            var order = GetValidOrder();
            order.EanCodeOfSupplier = 123;

            // Act
            var result = validator.TestValidate(order);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EanCodeOfSupplier);
        }
    }
}
