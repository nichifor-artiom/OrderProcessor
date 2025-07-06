using FluentValidation;
using OrderProcessor.Models;

public class ArticleValidator : AbstractValidator<Article>
{
    public ArticleValidator()
    {
        RuleFor(x => x.EanCode)
            .NotEmpty()
            .WithMessage("EAN code is required.")
            .Must(ec => ec.ToString().Length == 13)
            .WithMessage("EAN code must be exactly 13 digits.");

        RuleFor(x => x.Description)
            .NotEmpty()
            .Must(desc => !string.IsNullOrWhiteSpace(desc))
            .WithMessage("Description is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity is required.");

        RuleFor(x => x.UnitPrice)
            .GreaterThanOrEqualTo(0)
            .WithMessage("Unit price is required.");
    }
}