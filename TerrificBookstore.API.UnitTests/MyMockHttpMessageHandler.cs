using System.Net;
using System.Net.Http.Json;

namespace TerrificBookstore.API.UnitTests;

public class MyMockHttpMessageHandler: HttpMessageHandler
{
    private readonly object? _responseContent;
    private readonly HttpStatusCode _statusCode;

    public MyMockHttpMessageHandler(HttpStatusCode statusCode, object? responseContent)
    {
        _statusCode = statusCode;
        _responseContent = responseContent;
    }
    
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        return await Task.FromResult(new HttpResponseMessage
        {
            StatusCode = _statusCode,
            Content = JsonContent.Create(_responseContent)
        });
    }
}