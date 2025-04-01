namespace UMS.BuildingBlocks.Application.Messaging.Requests;

public interface IRequest;

public interface IRequest<out TResponse> : IRequest;