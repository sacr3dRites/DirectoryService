using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Departments.DeleteDepartment;

public record DeleteDepartmentCommand(Guid Id) : ICommand;