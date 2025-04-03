namespace UMS.BuildingBlocks.Application.Messaging.Requests;

public interface IRequestHandlerBase;

public interface IRequestHandler<in TRequest, TResponse> : IRequestHandlerBase
    where TRequest : IRequest<TResponse>
{
    Task<TResponse> Handle(TRequest request);
}

public interface IRequestHandler<in TRequest> : IRequestHandlerBase
    where TRequest : IRequest
{
    Task Handle(TRequest request);
}

