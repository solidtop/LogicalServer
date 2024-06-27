using LogicalServer.Configuration;
using LogicalServer.Examples;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddLogicalServer()
    .AddServerOptions(builder.Configuration);

// Examples
builder.MapHub<HelloHub>("/hello");

var host = builder.Build();
host.Run();
