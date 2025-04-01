using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Application.Messaging.Commands;

internal interface ICommandHandler<in TCommand> : IRequestHandler<TCommand>
    where TCommand : ICommand;