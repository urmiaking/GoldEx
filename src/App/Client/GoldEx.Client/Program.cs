using GoldEx.Client;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

builder.Services.AddClient(builder.HostEnvironment);

await builder.Build().RunAsync();
