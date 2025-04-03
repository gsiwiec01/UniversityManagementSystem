using Microsoft.Extensions.DependencyInjection;
using UMS.BuildingBlocks.Application.Messaging;
using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Infrastructure.Messaging;

internal class Mediator : IMediator
{
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task Send(IRequest request)
    {
        var handlerWrapperType = typeof(RequestHandlerWrapper<>).MakeGenericType(request.GetType());
        var handlerWrapper = (IRequestHandlerWrapper?) _serviceProvider.GetService(handlerWrapperType);
        if (handlerWrapper is null)
            throw new InvalidOperationException($"Handler for {request.GetType()} not found.");

        return handlerWrapper.Handle(request);
    }
    
    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        var handlerWrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(request.GetType(), typeof(TResponse));
        var handlerWrapper = (IRequestHandlerWrapper<TResponse>?) _serviceProvider.GetService(handlerWrapperType);
        if (handlerWrapper is null)
            throw new InvalidOperationException($"Handler for {request.GetType()} not found.");

        return handlerWrapper.Handle(request);
    }
}