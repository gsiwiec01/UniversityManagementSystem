using Microsoft.Extensions.DependencyInjection;
using UMS.BuildingBlocks.Application.Messaging.Requests;
using UMS.BuildingBlocks.Infrastructure.Messaging;

namespace UMS.BuildingBlocks.Tests.Infrastructure.Messaging;

public class MediatorTests
{
    [Fact]
    public async Task Should_Throw_Exception_When_Handler_Is_Not_Registered()
    {
        var serviceCollection = new ServiceCollection();
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var mediator = new Mediator(serviceProvider);
        
        var request = new TestRequest();
        await Assert.ThrowsAsync<InvalidOperationException>(() => mediator.Send(request));
    }
    
    [Fact]
    public async Task Should_Call_Handler()
    {
        var serviceCollection = new ServiceCollection();
        
        var handler = new TestRequestHandler();
        serviceCollection.AddSingleton<IRequestHandler<TestRequest>>(handler);
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequest();
        
        await mediator.Send(request);

        Assert.True(handler.Handled);
    }

    [Fact]
    public async Task Should_Call_Handler_With_Response()
    {
        var serviceCollection = new ServiceCollection();
        serviceCollection.AddTransient<IRequestHandler<TestRequestWithResponse, bool>, TestRequestWithResponseHandler>();
        serviceCollection.AddTransient<RequestHandlerWrapper<TestRequestWithResponse, bool>>();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        
        var mediator = new Mediator(serviceProvider);
        var request = new TestRequestWithResponse();
        
        var result = await mediator.Send(request);

        Assert.True(result);
    }
    
    private class TestRequest : IRequest;
    private class TestRequestHandler : IRequestHandler<TestRequest>
    {
        public bool Handled { get; private set; } = false;

        public Task Handle(TestRequest request)
        {
            Handled = true;
            return Task.CompletedTask;
        }
    }

    private class TestRequestWithResponse : IRequest<bool>;
    private class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, bool>
    {
        public Task<bool> Handle(TestRequestWithResponse request)
        {
            return Task.FromResult(true);
        }
    }
}