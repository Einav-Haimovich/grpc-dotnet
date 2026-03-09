using gRPC_In_Dotnet.Services;
using System.IO.Compression;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(
    options =>
    {
        options.ResponseCompressionAlgorithm = "gzip";
        options.ResponseCompressionLevel = CompressionLevel.SmallestSize;

    });

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<GreeterService>();
app.MapGrpcService<FirstService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");

app.Run();
