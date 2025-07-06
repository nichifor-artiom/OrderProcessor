using OrderProcessor.Models;
using System.Text;
using System.Xml.Serialization;

namespace OrderProcessor.Providers.Implementation;

public class OrderManagementSystemProvider : IOrderManagementSystemProvider
{
    private readonly HttpClient httpClient;

    public OrderManagementSystemProvider(HttpClient httpClient)
    {
        this.httpClient = httpClient;
    }

    public async Task SendOrderAsync(Order order)
    {
        var relativeUrl = "/orders";
        var request = new HttpRequestMessage(HttpMethod.Post, relativeUrl);

        var xmlContent = SerializeOrderToXml(order);
        request.Content = new StringContent(xmlContent, Encoding.UTF8, "application/xml");
        // auth??

        //var httpResponse = await this.httpClient.SendAsync(request);
        //await httpResponse.Content.ReadAsStringAsync();

        //if (!httpResponse.IsSuccessStatusCode)
        //{
        //    throw new Exception($"Failed to send order. Status code: {httpResponse.StatusCode}, Reason: {httpResponse.ReasonPhrase}");
        //}
    }

    public static string SerializeOrderToXml(Order order)
    {
        var serializer = new XmlSerializer(typeof(Order));
        using var stringWriter = new StringWriter();
        serializer.Serialize(stringWriter, order);
        return stringWriter.ToString();
    }
}
