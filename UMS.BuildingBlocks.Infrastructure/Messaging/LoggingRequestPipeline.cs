using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Infrastructure.Messaging;

public class LoggingRequestPipeline<TRequest, TResponse> : IRequestPipeline<TRequest, TResponse> 
    where TRequest : notnull
{
    public Task<TResponse> Process(TRequest request, Func<Task<TResponse>> next)
    {
        return next();
    }
}