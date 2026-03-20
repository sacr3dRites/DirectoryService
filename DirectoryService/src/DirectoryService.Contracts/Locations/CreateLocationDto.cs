namespace DirectoryService.Contracts.Locations;

public record CreateLocationDto(
    string Name,
    string Address,
    string Timezone);