using Basics;
using Grpc.Core;
using Grpc.Net.Client;
using static Grpc.Core.Metadata;

var options = new GrpcChannelOptions
{

};
using var channel = GrpcChannel.ForAddress("http://localhost:5210", options);

var client = new FirstServiceDefinition.FirstServiceDefinitionClient(channel);
//Unary(client);
//await ClientStreamingAsync(client);
//await ServerStreamingAsync(client);
await BiDirectionalStreamingAsync(client);

Console.ReadLine();
void Unary(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    var req = new Request
    {
        Content = "John Doe"
    };

    var result = client.Unary(req, deadline: DateTime.UtcNow.AddMilliseconds(3));
}

async Task ClientStreamingAsync(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var call = client.ClientStreaming();
    for (int i = 0; i < 5; i++)
    {
        var req = new Request
        {
            Content = $"Message {i}"
        };
        await call.RequestStream.WriteAsync(req);
    }
    await call.RequestStream.CompleteAsync();
    var result = await call.ResponseAsync;
    Console.WriteLine(result.Message);
}

async Task ServerStreamingAsync(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    try
    {
        var cts = new CancellationTokenSource();
        var metaData = new Metadata();

        metaData.Add(new Entry("Einav", "Haimovich"));



        var req = new Request
        {
            Content = "Einav Haimovich"
        };
        using var call = client.ServerStreaming(req, headers: metaData);
        await foreach (var res in call.ResponseStream.ReadAllAsync(cts.Token))
        {
            Console.WriteLine(res.Message);
            if (res.Message.Contains("2"))
            {
                cts.Cancel();
            }
        }

        var serverTrailers = call.GetTrailers();
        var trailerValue = serverTrailers.Get("Trailer");
    }
    catch (RpcException ex) when (ex.StatusCode == StatusCode.Cancelled)
    {
        
    }

    
}

async Task BiDirectionalStreamingAsync(FirstServiceDefinition.FirstServiceDefinitionClient client)
{
    using var call = client.BidirectionalStreaming();
    for (int i = 0; i < 5; i++)
    {
        var req = new Request
        {
            Content = $"Message {i}"
        };
        await call.RequestStream.WriteAsync(req);
    }

    while(await call.ResponseStream.MoveNext())
    {
        var message = call.ResponseStream.Current;
        Console.WriteLine(message);
    }

    await call.RequestStream.CompleteAsync();
}