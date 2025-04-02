using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using UMS.BuildingBlocks.Application.Messaging;
using UMS.BuildingBlocks.Application.Messaging.Requests;
using UMS.BuildingBlocks.Infrastructure.Messaging;
using Xunit;

namespace UMS.BuildingBlocks.Tests.Infrastructure.Messaging;

public class MediatorTests
{
    private readonly ServiceCollection _services;
    private readonly ServiceProvider _serviceProvider;
    private readonly Mediator _mediator;

    public MediatorTests()
    {
        _services = new ServiceCollection();
        _serviceProvider = _services.BuildServiceProvider();
        _mediator = new Mediator(_serviceProvider);
    }

    [Fact]
    public async Task Send_ShouldInvokeHandler_WhenHandlerExists()
    {
        // Arrange
        var request = new TestRequest();
        var handler = new TestRequestHandler();
        _services.AddSingleton<IRequestHandler<TestRequest>>(handler);
        _services.BuildServiceProvider();

        // Act
        await _mediator.Send(request);

        // Assert
        Assert.True(handler.Handled);
    }

    [Fact]
    public async Task Send_ShouldThrowException_WhenHandlerNotFound()
    {
        // Arrange
        var request = new TestRequest();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _mediator.Send(request));
    }

    [Fact]
    public async Task Send_WithResponse_ShouldInvokeHandler_WhenHandlerExists()
    {
        // Arrange
        var request = new TestRequestWithResponse();
        var handler = new TestRequestWithResponseHandler();
        _services.AddSingleton<IRequestHandler<TestRequestWithResponse, string>>(handler);
        _services.BuildServiceProvider();

        // Act
        var response = await _mediator.Send<TestRequestWithResponse, string>(request);

        // Assert
        Assert.Equal("Handled", response);
    }

    [Fact]
    public async Task Send_WithResponse_ShouldThrowException_WhenHandlerNotFound()
    {
        // Arrange
        var request = new TestRequestWithResponse();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _mediator.Send<TestRequestWithResponse, string>(request));
    }

    // Fake classes for testing
    private class TestRequest : IRequest { }

    private class TestRequestHandler : IRequestHandler<TestRequest>
    {
        public bool Handled { get; private set; }

        public Task Handle(TestRequest request)
        {
            Handled = true;
            return Task.CompletedTask;
        }
    }

    private class TestRequestWithResponse : IRequest<string> { }

    private class TestRequestWithResponseHandler : IRequestHandler<TestRequestWithResponse, string>
    {
        public Task<string> Handle(TestRequestWithResponse request)
        {
            return Task.FromResult("Handled");
        }
    }
}
