using UMS.BuildingBlocks.Application.Messaging.Requests;

namespace UMS.BuildingBlocks.Application.Messaging.Queries;

public interface IQuery<out TResult> : IRequest<TResult>;