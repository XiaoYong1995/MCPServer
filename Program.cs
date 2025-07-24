using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Client;
using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using SampleMcpServer.Tools;
using System.ComponentModel;
var builder = WebApplication.CreateBuilder(args);

// Configure all logs to go to stderr (stdout is used for the MCP protocol messages).
builder.Logging.AddConsole(o => o.LogToStandardErrorThreshold = LogLevel.Trace);

// Add the MCP services: the transport to use (stdio) and the tools to register.
builder.Services
    .AddMcpServer((P) =>
    {
        P.ServerInfo = new Implementation
        {
            Name = "Sample MCP Server",
            Version = "1.0.0",
            Title = "路由器操作的MCP服务器"
        };

    })
    .WithHttpTransport()
    .WithTools<RandomNumberTools>()
    .WithTools<RouterActionTools>();

var app = builder.Build();
app.MapMcp();
await app.RunAsync("http://localhost:3001");



