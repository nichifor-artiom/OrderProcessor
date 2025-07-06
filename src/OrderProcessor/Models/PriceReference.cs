namespace OrderProcessor.Models;

public class PriceReference
{
    public long ArticleCode { get; set; }
    public decimal DefaultPrice { get; set; }
    public decimal SpecificPrice { get; set; }
}
