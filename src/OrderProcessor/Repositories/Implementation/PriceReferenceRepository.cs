using OrderProcessor.Models;
using OrderProcessor.Models.BusinessModels;

namespace OrderProcessor.Repositories.Implementation;

public class PriceReferenceRepository : IPriceReferenceRepository
{
    private readonly List<DefaultPriceReference> defaultPriceReferences;
    private readonly List<SpecificPricaReference> specificPriceReferences;

    public PriceReferenceRepository()
    {
        this.defaultPriceReferences = new List<DefaultPriceReference>
        {
            new DefaultPriceReference { ArticleCode = 8712345678906, DefaultPrice = 123.35m },
            new DefaultPriceReference { ArticleCode = 8712345678913, DefaultPrice = 56.00m },
            new DefaultPriceReference { ArticleCode = 8712345678920, DefaultPrice = 13.90m },
        };
        this.specificPriceReferences = new List<SpecificPricaReference>
        {
            new SpecificPricaReference { ArticleCode = 8712345678913, BuyerCode = 8712345678937, SpecificPrice = 55.00m },
        };
    }

    public PriceReference GetPriceReference(long articleCode, long buyerCode)
    {
        // assuming that we can do a query on both sources
        var defaultPriceReference = this.defaultPriceReferences
            .FirstOrDefault(pr => pr.ArticleCode == articleCode);
        var specificPriceReference = this.specificPriceReferences
            .FirstOrDefault(pr => pr.ArticleCode == articleCode && pr.BuyerCode == buyerCode );

        return new PriceReference
        {
            ArticleCode = articleCode,
            DefaultPrice = defaultPriceReference?.DefaultPrice ?? 0,
            SpecificPrice = specificPriceReference?.SpecificPrice ?? 0
        };
    }
}
