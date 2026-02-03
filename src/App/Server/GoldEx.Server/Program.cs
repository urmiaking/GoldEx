using GoldEx.Sdk.Server.Infrastructure.Extensions;
using GoldEx.Server.Extensions;
using GoldEx.Server.Infrastructure;

try
{
    Console.WriteLine(@"Starting up.");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.ConfigureSerilog();
    Console.WriteLine($@"Environment: {builder.Environment.EnvironmentName}");

    var app = builder.ConfigureServices();

    app.ApplyDatabaseMigrations<GoldExDbContext>();

    await app.SeedDatabaseAsync();
    
    app.ConfigureSqlSerilog();

    await app.InitializeLicenseAsync();

    app.ConfigurePipeline();

    app.Run();
}
catch (Exception ex) when (ex is not HostAbortedException)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine(@"An error occured:");
    Console.WriteLine(ex.Message);
}
finally
{
    Console.ResetColor();
    Console.WriteLine(@"Shutting down completed.");
}