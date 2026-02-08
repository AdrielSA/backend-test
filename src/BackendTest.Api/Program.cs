using BackendTest.Api.Extensions;
using BackendTest.Api.Middleware;
using BackendTest.Application;
using BackendTest.Infrastructure;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(new ConfigurationBuilder()
        .AddJsonFile("appsettings.json")
        .Build())
    .Enrich.FromLogContext()
    .CreateLogger();

try
{
    Log.Information("Iniciando api...");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();

    builder.Services.AddControllers();

    builder.Services.AddApplicationServices();
    builder.Services.AddInfrastructureServices(builder.Configuration);

    builder.Services.AddOpenApi();

    var app = builder.Build();

    app.UseMiddleware<GlobalExceptionHandler>();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
    }

    app.UseSerilogRequestLogging();

    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();

    await app.InitializeDatabaseAsync(runSeeder: args.Contains("--seed"));

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "La api falló al iniciar");
}
finally
{
    Log.CloseAndFlush();
}
