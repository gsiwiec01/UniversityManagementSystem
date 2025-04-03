using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;
using UMS.BuildingBlocks.Application.Messaging;
using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Infrastructure.Messaging;

public class Mediator : IMediator
{
    private static readonly ConcurrentDictionary<Type, IRequestHandlerWrapperBase> RequestHandlers = new();
    
    private readonly IServiceProvider _serviceProvider;

    public Mediator(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public Task Send(IRequest request)
    {
        var handler = RequestHandlers.GetOrAdd(request.GetType(), type =>
        {
            var handlerWrapperType = typeof(RequestHandlerWrapper<>).MakeGenericType(type);
            var handlerWrapper = Activator.CreateInstance(handlerWrapperType, _serviceProvider);
            if (handlerWrapper is null)
                throw new InvalidOperationException($"Handler for {type} not found.");

            return (IRequestHandlerWrapperBase) handlerWrapper;
        });
        
        return handler.Handle(request);
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        var handler = (IRequestHandlerWrapper<TResponse>) RequestHandlers.GetOrAdd(request.GetType(), type =>
        {
            var handlerWrapperType = typeof(RequestHandlerWrapper<,>).MakeGenericType(type, typeof(TResponse));
            var handlerWrapper = Activator.CreateInstance(handlerWrapperType, _serviceProvider);
            if (handlerWrapper is null)
                throw new InvalidOperationException($"Handler for {type} not found.");

            return (IRequestHandlerWrapperBase) handlerWrapper;
        });
        
        return handler.Handle(request);
    }
}