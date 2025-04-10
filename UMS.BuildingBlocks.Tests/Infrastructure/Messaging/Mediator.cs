﻿using Microsoft.Extensions.DependencyInjection;
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
    
    [Fact]
    public async Task Should_Invoke_Pipeline()
    {
        var serviceCollection = new ServiceCollection();

        var handler = new TestRequestWithResponseHandler();
        var pipeline = new TestPipeline<TestRequestWithResponse, bool>();

        serviceCollection.AddSingleton<IRequestHandler<TestRequestWithResponse, bool>>(handler);
        serviceCollection.AddSingleton<IRequestPipeline<TestRequestWithResponse, bool>>(pipeline);
        serviceCollection.AddTransient<RequestHandlerWrapper<TestRequestWithResponse, bool>>();

        var serviceProvider = serviceCollection.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);

        var request = new TestRequestWithResponse();

        var result = await mediator.Send(request);

        Assert.True(result);
        Assert.True(pipeline.Handled);
    }


    [Fact]
    public async Task Should_Invoke_Pipeline_And_Handler_In_Correct_Order()
    {
        var serviceCollection = new ServiceCollection();
        var log = new List<string>();

        serviceCollection.AddSingleton(log);
        serviceCollection.AddSingleton(typeof(IRequestPipeline<,>), typeof(TestPipeline1<,>));
        serviceCollection.AddSingleton(typeof(IRequestPipeline<,>), typeof(TestPipeline2<,>));
        serviceCollection.AddTransient<IRequestHandler<TestPipelineRequest>, TestPipelineRequestHandler>();
        
        var serviceProvider = serviceCollection.BuildServiceProvider();
        var mediator = new Mediator(serviceProvider);

        var request = new TestPipelineRequest();
        await mediator.Send(request);
        
        var expectedOrder = new List<string>
        {
            "Pipeline1-Before",
            "Pipeline2-Before",
            "Handler",
            "Pipeline2-After",
            "Pipeline1-After"
        };
        
        Assert.Equal(expectedOrder, log);
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
    
    private class TestPipeline<TRequest, TResponse> : IRequestPipeline<TRequest, TResponse> 
        where TRequest : notnull
    {
        public bool Handled { get; private set; } = false;
        
        public async Task<TResponse> Process(TRequest request, Func<Task<TResponse>> next)
        {
            Handled = true;
            var result = await next();
            return result;
        }
    }
    
    
    private class TestPipeline1<TRequest, TResponse> : IRequestPipeline<TRequest, TResponse> 
        where TRequest : notnull
    {
        private readonly List<string> _log;
        public TestPipeline1(List<string> log) => _log = log;
        
        public async Task<TResponse> Process(TRequest request, Func<Task<TResponse>> next)
        {
            _log.Add("Pipeline1-Before");
            var result = await next();
            _log.Add("Pipeline1-After");
            return result;
        }
    }
    
    private class TestPipeline2<TRequest, TResponse> : IRequestPipeline<TRequest, TResponse> 
        where TRequest : notnull
    {
        private readonly List<string> _log;
        public TestPipeline2(List<string> log) => _log = log;
        
        public async Task<TResponse> Process(TRequest request, Func<Task<TResponse>> next)
        {
            _log.Add("Pipeline2-Before");
            var result = await next();
            _log.Add("Pipeline2-After");
            return result;
        }
    }


    private class TestPipelineRequest : IRequest;
    private class TestPipelineRequestHandler : IRequestHandler<TestPipelineRequest>
    {
        private readonly List<string> _log;
        public TestPipelineRequestHandler(List<string> log) => _log = log;
        
        public Task Handle(TestPipelineRequest request)
        {
            _log.Add("Handler");
            return Task.CompletedTask;
        }
    }
    
}