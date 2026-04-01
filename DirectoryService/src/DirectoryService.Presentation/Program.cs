using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Locations;
using DirectoryService.Presentation.Configuration;
using DirectoryService.Shared.CustomErrors;
using Microsoft.AspNetCore.Mvc;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    Log.Information("Starting web application");
    var builder = WebApplication.CreateBuilder(args);


    builder.Services.AddConfiguration(builder.Configuration);
    builder.Services.AddControllers();

    builder.Services.Configure<ApiBehaviorOptions>(options =>
    {
        options.SuppressModelStateInvalidFilter = true;
    });

    builder.Services.AddDirectoryService(builder.Configuration);

    builder.Services.AddScoped<ICommandHandler<Result<Guid, Errors>, CreateLocationCommand>, CreateLocationHandler>();
    builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();


    var app = builder.Build();

    app.AddExceptionMiddleware();
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();
        app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
    }

    app.MapControllers();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}