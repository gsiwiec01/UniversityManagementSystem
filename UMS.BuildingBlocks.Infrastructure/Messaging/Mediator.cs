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

    public Task Send<TRequest>(TRequest request) where TRequest : IRequest
    {
        var handler = _serviceProvider.GetService<IRequestHandler<IRequest>>();
        if (handler is null)
            throw new InvalidOperationException($"Handler for {request.GetType()} not found.");

        var processors = _serviceProvider.GetRequiredService<IEnumerable<IRequestPipeline<TRequest, Unit>>>();
        var resultHandler = processors
            .Reverse()
            .Aggregate(
                Handler,
                (next, processor) => () => processor.Process(request, next)
            );

        return resultHandler();

        async Task<Unit> Handler()
        {
            await handler.Handle(request);
            return new Unit();
        }
    }

    public Task<TResponse> Send<TRequest, TResponse>(TRequest request) where TRequest : IRequest<TResponse>
    {
        var handler = _serviceProvider.GetService<IRequestHandler<IRequest<TResponse>, TResponse>>();
        if (handler is null)
            throw new InvalidOperationException($"Handler for {request.GetType()} not found.");
        
        var processors = _serviceProvider.GetRequiredService<IEnumerable<IRequestPipeline<TRequest, TResponse>>>();
        var resultHandler = processors
            .Reverse()
            .Aggregate(
                () => handler.Handle(request),
                (next, processor) => () => processor.Process(request, next)
            );

        return resultHandler();
    }
}