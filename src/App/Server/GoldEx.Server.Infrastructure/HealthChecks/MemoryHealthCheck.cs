using Humanizer;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace GoldEx.Server.Infrastructure.HealthChecks;

public class MemoryHealthCheck(IOptionsMonitor<MemoryCheckOptions> options) : IHealthCheck
{
    public string Name => "memory_check";

    public Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        var options1 = options.Get(context.Registration.Name);

        // Include GC information in the reported diagnostics.
        var allocated = GC.GetTotalMemory(forceFullCollection: false);
        var data = new Dictionary<string, object>
        {
            { "AllocatedBytes", allocated },
            { "Gen0Collections", GC.CollectionCount(0) },
            { "Gen1Collections", GC.CollectionCount(1) },
            { "Gen2Collections", GC.CollectionCount(2) }
        };
        var status = allocated < options1.Threshold ? HealthStatus.Healthy : HealthStatus.Unhealthy;

        return Task.FromResult(new HealthCheckResult(
            status,
            description: $"{allocated.Bytes()} of {options1.Threshold.Bytes()} Used. Total free memory : {(options1.Threshold - allocated).Bytes()}",
            exception: null,
            data: data));
    }
}
public class MemoryCheckOptions
{
    public string MemoryStatus { get; set; } = default!;
    //public int Threshold { get; set; }
    // Failure threshold (in bytes)
    public long Threshold { get; set; } = 1024L * 1024L * 1024L;
}