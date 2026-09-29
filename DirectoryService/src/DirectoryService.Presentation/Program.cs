using DirectoryService.Application;
using DirectoryService.Application.Locations;
using DirectoryService.Infrastructure;
using DirectoryService.Infrastructure.Locations;
using DirectoryService.Presentation.Configuration;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

Dapper.DefaultTypeMap.MatchNamesWithUnderscores = true;

builder.Services.AddControllers();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});

builder.Services.AddConfiguration(builder.Configuration);
builder.Services.AddApplication(builder.Configuration);
builder.Services.AddDirectoryService(builder.Configuration);
builder.Services.AddInfrastructure(builder.Configuration);


var app = builder.Build();
app.AddExceptionMiddleware();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options => options.SwaggerEndpoint("/openapi/v1.json", "DirectoryService"));
}

app.UseRouting();
app.UseFrontendCors();
app.MapControllers();

app.Run();

public partial class Program;