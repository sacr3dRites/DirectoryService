namespace DirectoryService.Contracts.Locations;

public sealed record LocationDto(
    Guid Id,
    string Name,
    string Address,
    string Timezone,
    bool IsActive,
    DateTime CreatedAt,
    DateTime UpdatedAt);