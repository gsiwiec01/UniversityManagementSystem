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
        var handler = _serviceProvider.GetService<IRequestHandler<IRequest>>();
        if (handler is null)
            throw new InvalidOperationException($"Handler for {request.GetType()} not found.");
        
        return handler.Handle(request);
    }

    public Task<TResponse> Send<TResponse>(IRequest<TResponse> request)
    {
        var handler = _serviceProvider.GetService<IRequestHandler<IRequest<TResponse>, TResponse>>();
        if (handler is null)
            throw new InvalidOperationException($"Handler for {request.GetType()} not found.");
        
        return handler.Handle(request);
    }
}