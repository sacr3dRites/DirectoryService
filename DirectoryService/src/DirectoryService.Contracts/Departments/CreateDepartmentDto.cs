namespace DirectoryService.Contracts.Departments;

public record CreateDepartmentDto(string Name, string Identifier, Guid[] LocationIds, Guid? ParentId = null);