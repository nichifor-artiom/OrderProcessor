using FluentValidation;
using OrderProcessor.Models;

public class OrderValidator : AbstractValidator<Order>
{
    public OrderValidator()
    {
        RuleFor(x => x.FileTypeIdentifier)
            .NotEmpty()
            .Must(fti => fti == "ORD")
            .WithMessage("File type identifier should be always ORD");

        RuleFor(x => x.OrderNumber)
            .GreaterThan(0)
            .WithMessage("Order number must is required.");

        RuleFor(x => x.OrderDate)
            .NotEmpty()
            .WithMessage("Order date is required.");

        RuleFor(x => x.EanCodeOfBuyer)
            .GreaterThan(0)
            .WithMessage("Buyer EAN code is required.")
            .Must(ec => ec.ToString().Length == 13)
            .WithMessage("EAN code must be exactly 13 digits.");

        RuleFor(x => x.EanCodeOfSupplier)
            .GreaterThan(0)
            .WithMessage("Supplier EAN code is required.")
            .Must(ec => ec.ToString().Length == 13)
            .WithMessage("EAN code must be exactly 13 digits."); ;

        RuleForEach(x => x.Articles)
            .SetValidator(new ArticleValidator());
    }
}