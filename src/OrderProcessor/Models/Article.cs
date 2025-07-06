namespace OrderProcessor.Models;

public class Article
{
    public long EanCode { get; set; }

    public string Description { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }
}
