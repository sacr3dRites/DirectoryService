namespace DirectoryService.Contracts.Locations;

public record CreateLocationRequest(
    string Name,
    string Address,
    string Timezone);