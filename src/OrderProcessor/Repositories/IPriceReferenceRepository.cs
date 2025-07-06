using OrderProcessor.Models;

namespace OrderProcessor.Repositories;

public interface IPriceReferenceRepository
{
    PriceReference GetPriceReference(long articleCode, long buyerCode);
}
