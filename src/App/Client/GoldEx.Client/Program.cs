using GoldEx.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddClient(builder.HostEnvironment);

var app = builder.Build();

await app.RunAsync();