using FluentValidation.TestHelper;
using OrderProcessor.Models;
using Xunit;

namespace OrderProcess.Unit.Tests.Validation
{
    public class ArticleValidatorTest
    {
        private readonly ArticleValidator validator = new();

        private Article GetValidArticle() => new Article
        {
            EanCode = 1234567890123,
            Description = "Test Article",
            Quantity = 1,
            UnitPrice = 10.0m
        };

        [Fact]
        public void ValidArticle_ShouldPass()
        {
            // Arrange, Act
            var result = validator.TestValidate(GetValidArticle());

            // Assert
            result.ShouldNotHaveAnyValidationErrors();
        }

        [Fact]
        public void EanCode_InvalidLength_ShouldFail()
        {
            // Arrange
            var article = GetValidArticle();
            article.EanCode = 123;

            // Act
            var result = validator.TestValidate(article);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.EanCode);
        }

        [Theory]
        [InlineData("")]
        [InlineData("   ")]
        public void Description_Invalid_ShouldFail(string description)
        {
            // Arrange
            var article = GetValidArticle();
            article.Description = description;

            // Act
            var result = validator.TestValidate(article);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Description);
        }

        [Fact]
        public void Quantity_LessThanOrEqualToZero_ShouldFail()
        {
            // Arrange
            var article = GetValidArticle();
            article.Quantity = 0;

            // Act
            var result = validator.TestValidate(article);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.Quantity);
        }

        [Fact]
        public void UnitPrice_Negative_ShouldFail()
        {
            // Arrange
            var article = GetValidArticle();
            article.UnitPrice = -1.0m;

            // Act
            var result = validator.TestValidate(article);

            // Assert
            result.ShouldHaveValidationErrorFor(x => x.UnitPrice);
        }
    }
}
