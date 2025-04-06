using Microsoft.Extensions.DependencyInjection;
using UMS.BuildingBlocks.Application.Messaging;
using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Infrastructure.Messaging;

internal class RequestHandlerWrapper<TRequest, TResponse> : IRequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    public async Task<object?> Handle(object request, IServiceProvider serviceProvider)
    {
        return await Handle((TRequest) request, serviceProvider);
    }

    public Task<TResponse> Handle(IRequest<TResponse> request, IServiceProvider serviceProvider)
    {
        var requestHandler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        var pipelines = serviceProvider.GetRequiredService<IEnumerable<IRequestPipeline<TRequest, TResponse>>>();
        
        var handler = pipelines
            .Reverse()
            .Aggregate(
                () => requestHandler.Handle((TRequest) request),
                (next, pipeline) => () => pipeline.Process((TRequest) request, next)
            )();

        return handler;
    }
}

public class RequestHandlerWrapper<TRequest> : IRequestHandlerWrapper
    where TRequest : IRequest
{
    public async Task<object?> Handle(object request, IServiceProvider serviceProvider)
    {
        await Handle((TRequest) request, serviceProvider);
        return Unit.Value;
    }

    public Task Handle(IRequest request, IServiceProvider serviceProvider)
    {
        var requestHandler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        var pipelines = serviceProvider.GetRequiredService<IEnumerable<IRequestPipeline<TRequest, Unit>>>();
        
        return pipelines
            .Reverse()
            .Aggregate(
                Handler,
                (next, processor) => async () => await processor.Process((TRequest) request, next)
            )();

        async Task<Unit> Handler()
        {
            await requestHandler.Handle((TRequest) request);
            return Unit.Value;
        }
    }
}