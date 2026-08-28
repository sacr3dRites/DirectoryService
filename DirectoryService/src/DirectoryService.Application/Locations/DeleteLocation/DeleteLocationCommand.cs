using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Locations.DeleteLocation;

public record DeleteLocationCommand(Guid Id) : ICommand;