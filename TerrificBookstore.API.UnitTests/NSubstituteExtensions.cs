using System.Net;
using System.Net.Http.Json;
using System.Reflection;
using FluentAssertions;
using FluentAssertions.Execution;
using NSubstitute;
using NSubstitute.Core;

namespace TerrificBookstore.API.UnitTests;

public static class NSubstituteExtensions
{
    public static HttpMessageHandler SetupRequest(this HttpMessageHandler handler, HttpMethod method, string requestUri)
    {
        handler
            .GetType()
            .GetMethod("SendAsync", BindingFlags.NonPublic | BindingFlags.Instance)!
            .Invoke(handler, new object?[]
            {
                Arg.Is<HttpRequestMessage>(x =>
                    x.Method == method &&
                    x.RequestUri != null &&
                    x.RequestUri.ToString() == requestUri),
                Arg.Any<CancellationToken>()
            });

        return handler;
    }

    public static ConfiguredCall ReturnsResponse(this HttpMessageHandler handler, HttpStatusCode statusCode,
        object? responseContent = null)
    {
        return ((object)handler).Returns(
            Task.FromResult(new HttpResponseMessage()
            {
                StatusCode = statusCode,
                Content = JsonContent.Create(responseContent)
            })
        );
    }

    public static void ShouldHaveReceived(this HttpMessageHandler handler, HttpMethod requestMethod, string requestUri, int timesCalled = 1)
    {
        var calls = handler.ReceivedCalls()
            .Where(call => call.GetMethodInfo().Name == "SendAsync")
            .Select(call => call.GetOriginalArguments().First())
            .Cast<HttpRequestMessage>()
            .Where(request =>
                request.Method == requestMethod &&
                request.RequestUri != null &&
                request.RequestUri.ToString() == requestUri
            );

        calls.Should().HaveCount(timesCalled, $"HttpMessageHandler was expected to make the following call {timesCalled} times: {requestMethod} {requestUri}");
    }
}

public static class ArgIs
{
    private static bool MatchingAssertion(Action assertion)
    {
        using var assertionScope = new AssertionScope();
        assertion();
        return !assertionScope.Discard().Any();
    }

    public static T EquivalentTo<T>(T value) where T : class
    {
        return Arg.Is<T>(arg => MatchingAssertion(() => arg.Should().BeEquivalentTo(value, string.Empty)));
    }
}
