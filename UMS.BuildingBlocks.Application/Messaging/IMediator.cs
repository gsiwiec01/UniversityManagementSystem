using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Application.Messaging;

public interface IMediator
{
    Task Send<TRequest>(TRequest command) 
        where TRequest : IRequest;
    
    Task<TResponse> Send<TRequest, TResponse>(TRequest request) 
        where TRequest : IRequest<TResponse>;
}