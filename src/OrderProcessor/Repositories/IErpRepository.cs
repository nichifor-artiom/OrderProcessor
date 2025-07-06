using OrderProcessor.Models.BusinessModels;

namespace OrderProcessor.Repositories;

public interface IErpRepository
{
    StockItem GetStockItem(long articleNumber);
    void UpsertStockItem(StockItem stockItem);
}
