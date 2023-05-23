using System.Net.Http.Headers;
using GraphQL.Client.Abstractions;
using GraphQL.Client.Http;

public class AuthenticatedGraphQLHttpRequest : GraphQLHttpRequest
{
  private readonly string serviceAccountToken;

  public AuthenticatedGraphQLHttpRequest(string serviceAccountToken, string query, object? variables, string? operationName)
    : base(query, variables, operationName)
  {
    this.serviceAccountToken = serviceAccountToken;
  }

  public override HttpRequestMessage ToHttpRequestMessage(GraphQLHttpClientOptions options, IGraphQLJsonSerializer serializer)
  {
    var r = base.ToHttpRequestMessage(options, serializer);
    r.Headers.Authorization = new AuthenticationHeaderValue("ServiceAccount", this.serviceAccountToken);

    return r;
  }
}