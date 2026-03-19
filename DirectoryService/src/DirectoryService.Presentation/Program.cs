using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Locations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Locations;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddDirectoryService(builder.Configuration);

builder.Services.AddScoped<ICommandHandler<Guid, CreateLocationCommand>, CreateLocationHandler>();
builder.Services.AddScoped<ILocationsRepository, LocationsRepository>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapControllers();

app.Run();