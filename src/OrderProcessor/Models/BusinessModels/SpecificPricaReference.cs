namespace OrderProcessor.Models.BusinessModels;

public class SpecificPricaReference
{
    public long ArticleCode { get; set; }
    public long BuyerCode { get; set; }
    public decimal? SpecificPrice { get; set; }
}
