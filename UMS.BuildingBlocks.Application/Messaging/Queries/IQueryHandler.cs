using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Application.Messaging.Queries;

internal interface IQueryHandler<in TRequest, TResponse> : IRequestHandler<TRequest, TResponse>
    where TRequest : IQuery<TResponse>;