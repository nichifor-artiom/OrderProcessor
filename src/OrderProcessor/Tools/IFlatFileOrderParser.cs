using OrderProcessor.Models;

namespace OrderProcessor.Tools;

public interface IFlatFileOrderParser
{
    Order ParseOrderFile(string filePath);
}
