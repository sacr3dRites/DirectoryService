using DirectoryService.Application.Abstractions;
using DirectoryService.Contracts.Positions;

namespace DirectoryService.Application.Positions.CreatePosition;

public record CreatePositionCommand(CreatePositionDto CreatePositionDto) : ICommand;