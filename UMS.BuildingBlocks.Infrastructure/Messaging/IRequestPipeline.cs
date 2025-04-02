using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Infrastructure.Messaging;

public interface IRequestPipeline<in TRequest, TResponse>
    where TRequest : notnull
{
    Task<TResponse> Process(TRequest request, Func<Task<TResponse>> next);
}