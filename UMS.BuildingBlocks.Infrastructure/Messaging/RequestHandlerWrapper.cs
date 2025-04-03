using Microsoft.Extensions.DependencyInjection;
using UMS.BuildingBlocks.Application.Messaging;
using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Infrastructure.Messaging;

internal class RequestHandlerWrapper<TRequest, TResponse> : IRequestHandlerWrapper<TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IRequestHandler<TRequest, TResponse> _requestHandler;
    private readonly IEnumerable<IRequestPipeline<TRequest, TResponse>> _pipelines;
    
    public RequestHandlerWrapper(IServiceProvider serviceProvider)
    {
        _requestHandler = serviceProvider.GetRequiredService<IRequestHandler<TRequest, TResponse>>();
        _pipelines = serviceProvider.GetRequiredService<IEnumerable<IRequestPipeline<TRequest, TResponse>>>();
    }

    public async Task<object?> Handle(object request)
    {
        return await Handle((TRequest) request);
    }

    public Task<TResponse> Handle(IRequest<TResponse> request)
    {
        var handler = _pipelines
            .Reverse()
            .Aggregate(
                () => _requestHandler.Handle((TRequest) request),
                (next, pipeline) => () => pipeline.Process((TRequest) request, next)
            )();

        return handler;
    }
}

public class RequestHandlerWrapper<TRequest> : IRequestHandlerWrapper
    where TRequest : IRequest
{
    private readonly IRequestHandler<TRequest> _requestHandler;
    private readonly IEnumerable<IRequestPipeline<TRequest, Unit>> _pipelines;

    public RequestHandlerWrapper(IServiceProvider serviceProvider)
    {
        _requestHandler = serviceProvider.GetRequiredService<IRequestHandler<TRequest>>();
        _pipelines = serviceProvider.GetRequiredService<IEnumerable<IRequestPipeline<TRequest, Unit>>>();
    }

    public async Task<object?> Handle(object request)
    {
        await Handle((TRequest) request);
        return new Unit();
    }

    public Task Handle(IRequest request)
    {
        return _pipelines
            .Reverse()
            .Aggregate(
                Handler,
                (next, processor) => async () => await processor.Process((TRequest) request, next)
            )();

        async Task<Unit> Handler()
        {
            await _requestHandler.Handle((TRequest) request);
            return new Unit();
        }
    }
}