namespace DirectoryService.Domain.Departments.ValueObjects;

public record DepartmentPath
{
    private DepartmentPath(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static DepartmentPath Create(DepartmentIdentifier identifier, Department? parent)
    {
        if (parent is null)
            return new DepartmentPath(identifier.Value);

        var path = $"{parent.Path.Value}.{identifier.Value}";

        return new DepartmentPath(path);
    }
}