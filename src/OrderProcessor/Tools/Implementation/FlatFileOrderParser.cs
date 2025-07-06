using OrderProcessor.Models;
using System.Globalization;

namespace OrderProcessor.Tools.Implementation;

public class FlatFileOrderParser : IFlatFileOrderParser
{
    private const string OrderDateFormat = "yyyyMMddTHHmm";

    private static readonly Dictionary<string, (int start, int length)> OrderHeaderColumns = new()
        {
            { nameof(Order.FileTypeIdentifier), (0, 3) },
            { nameof(Order.OrderNumber), (3, 20) },
            { nameof(Order.OrderDate), (23, 13) },
            { nameof(Order.EanCodeOfBuyer), (36, 13) },
            { nameof(Order.EanCodeOfSupplier), (49, 13) },
            { nameof(Order.Comment), (62, 100) }
        };

    private static readonly Dictionary<string, (int start, int length)> OrderLineColumns = new()
        {
            { nameof(Article.EanCode), (0, 13) },
            { nameof(Article.Description), (13, 65) },
            { nameof(Article.Quantity), (78, 10) },
            { nameof(Article.UnitPrice), (88, 10) }
        };

    public Order ParseOrderFile(string filePath)
    {
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Order file not found: {filePath}");
        }

        var lines = File.ReadAllLines(filePath);
        if (lines.Length < 2)
        {
            throw new InvalidDataException("Order file must contain at least header and one line item");
        }

        var order = ParseHeader(lines[0]);

        // Parse order lines (skip header)
        for (int i = 1; i < lines.Length; i++)
        {
            if (!string.IsNullOrWhiteSpace(lines[i]))
            {
                var orderLine = ParseOrderLine(lines[i]);
                order.Articles.Add(orderLine);
            }
        }

        return order;
    }

    private Order ParseHeader(string headerLine)
    {
        if (string.IsNullOrWhiteSpace(headerLine))
        {
            throw new InvalidDataException("Invalid header line format");
        }

        return new Order
        {
            FileTypeIdentifier = ExtractField(headerLine, OrderHeaderColumns[nameof(Order.FileTypeIdentifier)]).Trim(),
            OrderNumber = decimal.Parse(ExtractField(headerLine, OrderHeaderColumns[nameof(Order.OrderNumber)]).Trim()),
            OrderDate = ParseDate(ExtractField(headerLine, OrderHeaderColumns[nameof(Order.OrderDate)])),
            EanCodeOfBuyer = long.Parse(ExtractField(headerLine, OrderHeaderColumns[nameof(Order.EanCodeOfBuyer)]).Trim()),
            EanCodeOfSupplier = long.Parse(ExtractField(headerLine, OrderHeaderColumns[nameof(Order.EanCodeOfSupplier)]).Trim()),
            Comment = ExtractField(headerLine, OrderHeaderColumns[nameof(Order.Comment)]).Trim()
        };
    }

    private Article ParseOrderLine(string orderLine)
    {
        if (string.IsNullOrWhiteSpace(orderLine) || orderLine.Length < 73)
        {
            throw new InvalidDataException($"Invalid order line format: {orderLine}");
        }

        return new Article
        {
            EanCode = long.Parse(ExtractField(orderLine, OrderLineColumns[nameof(Article.EanCode)]).Trim()),
            Description = ExtractField(orderLine, OrderLineColumns[nameof(Article.Description)]).Trim(),
            Quantity = int.Parse(ExtractField(orderLine, OrderLineColumns[nameof(Article.Quantity)]).Trim()),
            UnitPrice = decimal.Parse(ExtractField(orderLine, OrderLineColumns[nameof(Article.UnitPrice)]).Trim()),
        };
    }

    private string ExtractField(string line, (int start, int length) column)
    {
        if (line.Length < column.start + column.length)
        {
            throw new InvalidDataException($"Line too short to extract field at position {column.start} with length {column.length}");
        }

        return line.Substring(column.start, column.length);
    }

    private DateTime ParseDate(string dateString)
    {
        dateString = dateString.Trim();
        if (DateTime.TryParseExact(dateString, OrderDateFormat, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime result))
        {
            return result;
        }

        throw new InvalidDataException($"Invalid date format: {dateString}. Expected {OrderDateFormat}");
    }
}
