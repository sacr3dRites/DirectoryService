using DirectoryService.Presentation.Middlewares;
using DirectoryService.Shared.CustomErrors;
using DirectoryService.Shared.EndpointResults;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Exceptions;

namespace DirectoryService.Presentation.Configuration;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        return services
            .AddSerilogLogging(configuration)
            .AddOpenApiSpec();
    }

    public static IApplicationBuilder AddExceptionMiddleware(this IApplicationBuilder app)
    {
        return app.UseMiddleware<ExceptionMiddleware>();
    }

    private static IServiceCollection AddOpenApiSpec(this IServiceCollection services)
    {
        services.AddOpenApi();

        services.AddOpenApi(options =>
        {
            options.AddSchemaTransformer((schema, context, _) =>
            {
                if (context.JsonTypeInfo.Type == typeof(Envelope<Errors>))
                {
                    if (schema.Properties.TryGetValue("errors", out var errorsProp))
                    {
                        errorsProp.Items.Reference =
                            new OpenApiReference { Type = ReferenceType.Schema, Id = "Error", };
                    }
                }

                return Task.CompletedTask;
            });
        });

        return services;
    }

    private static IServiceCollection AddSerilogLogging(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSerilog((services, lc) => lc
            .ReadFrom.Configuration(configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .Enrich.WithExceptionDetails()
            .Enrich.WithProperty("ServiceName", "DirectoryService"));

        return services;
    }
}