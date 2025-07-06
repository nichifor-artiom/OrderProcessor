namespace OrderProcessor.Models;

public class Order
{
    public string FileTypeIdentifier { get; set; }

    public decimal OrderNumber { get; set; }

    public DateTime OrderDate { get; set; }

    public long EanCodeOfBuyer { get; set; }

    public long EanCodeOfSupplier { get; set; }

    public string Comment { get; set; }

    public List<Article> Articles { get; set; } = new List<Article>();
}
