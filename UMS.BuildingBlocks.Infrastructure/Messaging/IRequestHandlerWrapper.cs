using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Infrastructure.Messaging;

public interface IRequestHandlerWrapperBase
{
    Task<object?> Handle(object request, IServiceProvider serviceProvider);
}

public interface IRequestHandlerWrapper : IRequestHandlerWrapperBase
{
    Task Handle(IRequest request, IServiceProvider serviceProvider);
}

internal interface IRequestHandlerWrapper<TResponse> : IRequestHandlerWrapperBase
{
    Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider);
}