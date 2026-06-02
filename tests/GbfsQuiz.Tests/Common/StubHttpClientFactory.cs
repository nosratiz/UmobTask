namespace GbfsQuiz.Tests.Common;

/// <summary>
/// Minimal <see cref="IHttpClientFactory"/> that always hands back the same
/// <see cref="HttpClient"/>, letting tests construct clients that take a factory
/// without spinning up the full DI container.
/// </summary>
public sealed class StubHttpClientFactory(HttpClient client) : IHttpClientFactory
{
    public HttpClient CreateClient(string name) => client;
}
