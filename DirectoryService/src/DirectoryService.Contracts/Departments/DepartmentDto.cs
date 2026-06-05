namespace DirectoryService.Contracts.Departments;

public sealed record DepartmentDto(
    Guid Id,
    string Name,
    string Identifier,
    string Path,
    Guid? ParentId,
    short Depth,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);