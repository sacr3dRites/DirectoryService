namespace DirectoryService.Contracts.Departments;

public sealed record DepartmentDto
{
    public DepartmentDto(Guid Id,
        string Name,
        string Identifier,
        string Path,
        Guid? ParentId,
        short Depth,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt)
    {
        this.Id = Id;
        this.Name = Name;
        this.Identifier = Identifier;
        this.Path = Path;
        this.ParentId = ParentId;
        this.Depth = Depth;
        this.IsActive = IsActive;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
    }

    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Identifier { get; init; }
    public string Path { get; init; }
    public Guid? ParentId { get; init; }
    public short Depth { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}