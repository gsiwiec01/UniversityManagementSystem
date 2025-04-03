namespace UMS.BuildingBlocks.Application.Messaging.Requests;

public interface IRequest : IBaseRequest;

public interface IRequest<out TResponse> : IBaseRequest;