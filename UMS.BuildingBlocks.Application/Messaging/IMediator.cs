using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Application.Messaging;

public interface IMediator
{
    Task Send(IRequest request);
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request);
}