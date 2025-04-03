using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Infrastructure.Messaging;

internal interface IRequestHandlerWrapperBase
{
    Task<object?> Handle(object request);
}

internal interface IRequestHandlerWrapper : IRequestHandlerWrapperBase
{
    Task Handle(IRequest request);
}


internal interface IRequestHandlerWrapper<TResponse> : IRequestHandlerWrapperBase
{
    Task<TResponse> Handle(IRequest<TResponse> request);
}