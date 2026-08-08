namespace DirectoryService.Contracts.Departments;

public record DepartmentListItemDto(Guid Id, string Name, string Path, DateTime CreatedAt);