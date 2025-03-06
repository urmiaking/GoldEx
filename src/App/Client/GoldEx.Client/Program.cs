using GoldEx.Client;
using GoldEx.Client.Extensions;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddClient(builder.HostEnvironment);

var app = builder.Build();

await app.InitializeDbAsync();

await app.RunAsync();