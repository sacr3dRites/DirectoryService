namespace DirectoryService.Contracts.Locations;

public sealed record LocationDto
{
    public LocationDto(Guid Id,
        string Name,
        string Address,
        string Timezone,
        bool IsActive,
        DateTime CreatedAt,
        DateTime UpdatedAt)
    {
        this.Id = Id;
        this.Name = Name;
        this.Address = Address;
        this.Timezone = Timezone;
        this.IsActive = IsActive;
        this.CreatedAt = CreatedAt;
        this.UpdatedAt = UpdatedAt;
    }

    public Guid Id { get; init; }
    public string Name { get; init; }
    public string Address { get; init; }
    public string Timezone { get; init; }
    public bool IsActive { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}