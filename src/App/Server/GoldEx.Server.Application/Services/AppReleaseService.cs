using GoldEx.Sdk.Common.DependencyInjections;
using GoldEx.Shared.DTOs.AppReleases;
using GoldEx.Shared.Services.Abstractions;
using Microsoft.Extensions.Hosting;
using System.Text.Json;

namespace GoldEx.Server.Application.Services;

[SingletonService]
internal class AppReleaseService : IAppReleaseService
{
    private readonly List<AppReleaseResponse> _releases;

    public AppReleaseService(IHostEnvironment environment)
    {
        var path = Path.Combine(environment.ContentRootPath, "releases.json");
        var json = File.ReadAllText(path);

        _releases = JsonSerializer.Deserialize<List<AppReleaseResponse>>(
                json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                })!
            .OrderByDescending(r => new Version(r.Version))
            .ToList();

    }

    public Task<List<AppReleaseResponse>> GetListAsync(CancellationToken cancellationToken = default) =>
        Task.FromResult(_releases);
}