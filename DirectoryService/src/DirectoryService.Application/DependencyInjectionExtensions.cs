using CSharpFunctionalExtensions;
using DirectoryService.Application.Abstractions;
using DirectoryService.Application.Database;
using DirectoryService.Application.Departments.CreateDepartment;
using DirectoryService.Application.Departments.TransferDepartment;
using DirectoryService.Application.Departments.DeleteDepartment;
using DirectoryService.Application.Departments.GetAllDepartments;
using DirectoryService.Application.Departments.GetDepartment;
using DirectoryService.Application.Departments.UpdateDepartmentLocations;
using DirectoryService.Application.Locations.CreateLocation;
using DirectoryService.Application.Locations.DeleteLocation;
using DirectoryService.Application.Locations.GetLocation;
using DirectoryService.Application.Locations.GetLocations;
using DirectoryService.Application.Locations.GetTopLocations;
using DirectoryService.Application.PaginationUtils;
using DirectoryService.Application.Positions.CreatePosition;
using DirectoryService.Application.Positions.DeletePosition;
using DirectoryService.Contracts.Departments;
using DirectoryService.Contracts.Locations;
using DirectoryService.Shared.CustomErrors;
using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DirectoryService.Application;

public static class DependencyInjectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ICommandHandler<Result<Guid, Errors>, CreateLocationCommand>, CreateLocationHandler>();
        services.AddScoped<ICommandHandler<Result<Guid, Errors>, CreateDepartmentCommand>, CreateDepartmentHandler>();
        services
            .AddScoped<ICommandHandler<Result<Guid, Errors>, TransferDepartmentCommand>, TransferDepartmentHandler>();
        services.AddScoped<ICommandHandler<Result<Guid, Errors>, DeletePositionCommand>, DeletePositionHandler>();
        services.AddScoped<ICommandHandler<Result<Guid, Errors>, DeleteLocationCommand>, DeleteLocationHandler>();
        services.AddScoped<ICommandHandler<Result<Guid, Errors>, DeleteDepartmentCommand>, DeleteDepartmentHandler>();
        services.AddScoped<IQueryByIdHandler<DepartmentDto>, DepartmentQueryByIdHandler>();
        services.AddScoped<IQueryByIdHandler<LocationDto>, LocationQueryByIdHandler>();
        services.AddScoped<IQueryHandler<LocationsTopDto[]>, GetTopLocationsDapperHandler>();
        services
            .AddScoped<IQueryHandler<GetDepartmentsQuery, PagedResult<DepartmentListItemDto>>,
                GetAllDepartmentsHandler>();
        services.AddScoped<IQueryHandler<GetLocationsQuery, PagedResult<LocationListItemDto>>, GetLocationsHandler>();
        services.AddScoped<ICommandHandler<Result<Guid, Errors>, CreatePositionCommand>, CreatePositionHandler>();
        services
            .AddScoped<ICommandHandler<Result<Guid, Errors>, UpdateDepartmentLocationsCommand>,
                UpdateDepartmentLocationsHandler>();
        services.AddValidatorsFromAssembly(typeof(DependencyInjectionExtensions).Assembly);

        return services;
    }
}