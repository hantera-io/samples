

using GraphQL;
using GraphQL.Client.Abstractions.Websocket;
using GraphQL.Client.Http;
using GraphQL.Client.Serializer.Newtonsoft;
using Newtonsoft.Json;

public class LoggingGraphQLHttpClient
{
  private GraphQLHttpClient client;

  public LoggingGraphQLHttpClient(string endPoint)
  {
    client = new GraphQLHttpClient(endPoint, new NewtonsoftJsonSerializer());
  }

  public async Task<TResponse?> SendMutationAsync<TResponse>(GraphQL.GraphQLRequest request) where TResponse : class
  {
    Log.WriteLine("Sending request: ###########################################", ConsoleColor.Green);
    Log.WriteLine(request.Query);

    var res = await client.SendMutationAsync<TResponse>(request);

    if (res.Errors != null && res.Errors.Any())
    {
      Log.WriteLine("Request failed: " + string.Join(" ", res.Errors.Select(r => r.Message)), ConsoleColor.Red);
      return null;
    }

    Log.WriteLine("Response: ###########################################", ConsoleColor.Green);

    Log.WriteLine(JsonConvert.SerializeObject(res.Data, Formatting.Indented));

    Log.WriteLine("############################################################", ConsoleColor.Green);
    Log.WriteLine();

    return res.Data;
  }
}