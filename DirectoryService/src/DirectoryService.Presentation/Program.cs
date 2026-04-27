using DirectoryService.Application;
using DirectoryService.Application.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Locations;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddOpenApi();
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddDirectoryService(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.MapControllers();

app.Run();