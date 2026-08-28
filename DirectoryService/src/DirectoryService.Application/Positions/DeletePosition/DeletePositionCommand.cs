using DirectoryService.Application.Abstractions;

namespace DirectoryService.Application.Positions.DeletePosition;

public record DeletePositionCommand(Guid Id): ICommand;