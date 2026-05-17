using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Departments;

namespace DirectoryService.Application.Departments.TransferDepartment;

public record TransferDepartmentCommand(Guid DepartmentId, TransferDepartmentRequest TransferDepartmentRequest) : ICommand;