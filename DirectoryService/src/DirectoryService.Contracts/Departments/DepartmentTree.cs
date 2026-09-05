namespace DirectoryService.Contracts.Departments;

public record DepartmentTree(
    Guid Id,
    string Name,
    string Identifier,
    string Path,
    int Depth,
    bool HasChildren,
    int ChildrenCount
);