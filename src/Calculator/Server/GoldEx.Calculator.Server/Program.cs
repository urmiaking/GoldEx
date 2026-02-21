using GoldEx.Calculator.Server.Extensions;
using GoldEx.Sdk.Server.Infrastructure.Extensions;
using GoldEx.Server.Infrastructure;

try
{
    Console.WriteLine(@"Starting up.");

    var builder = WebApplication.CreateBuilder(args);

    Console.WriteLine($@"Environment: {builder.Environment.EnvironmentName}");

    var app = builder.ConfigureServices();

    app.ApplyDatabaseMigrations<GoldExDbContext>();

    await app.SeedDatabaseAsync();

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