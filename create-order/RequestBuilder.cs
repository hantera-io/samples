
using GraphQL;

public class RequestBuilder
{
  private readonly string token;

  public RequestBuilder(string token)
  {
    this.token = token;
  }

  public GraphQLRequest Build(string query, object? variables = null, string? operationName = null)
  {
    return new AuthenticatedGraphQLHttpRequest(token, query, variables, operationName);
  }
}