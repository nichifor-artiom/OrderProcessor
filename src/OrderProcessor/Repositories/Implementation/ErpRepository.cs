using OrderProcessor.Models.BusinessModels;

namespace OrderProcessor.Repositories.Implementation
{
    public class ErpRepository : IErpRepository
    {
        private readonly List<StockItem> stockItems;

        public ErpRepository()
        {
            this.stockItems = new List<StockItem>
            {
                new StockItem { ArticleCode = 8712345678906, AvailableQuantity = 15},
                new StockItem { ArticleCode = 8712345678913, AvailableQuantity = 15},
                new StockItem { ArticleCode = 8712345678920, AvailableQuantity = 500}
            };
        }

        public StockItem GetStockItem(long articleNumber)
        {
            return this.stockItems.FirstOrDefault(si => si.ArticleCode == articleNumber) ??
                   throw new KeyNotFoundException($"Stock item with article number {articleNumber} not found.");
        }

        public void UpsertStockItem(StockItem stockItem)
        {
            this.stockItems.Where(si => si.ArticleCode == stockItem.ArticleCode).ToList().ForEach(si =>
            {
                si.AvailableQuantity = stockItem.AvailableQuantity;
            });
        }
    }
}
