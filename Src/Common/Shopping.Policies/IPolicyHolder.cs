﻿using Microsoft.Extensions.Caching.Memory;
using Polly;
using Polly.Wrap;
using System;
using System.Net;
using System.Net.Http;

namespace Shopping.Policies
{
    public interface IPolicyHolder
    {
        IAsyncPolicy<HttpResponseMessage> CircuitBreakerPolicy(int allowedNumberOfAttemptsBeforeBreaking, TimeSpan durationOfBreak);
        IAsyncPolicy<HttpResponseMessage> FallbackPolicy();
        IAsyncPolicy<HttpResponseMessage> FallbackPolicy<TResponse>(HttpStatusCode statusCode, TResponse response) where TResponse : class;
        IPolicyWrap<HttpResponseMessage> FallbackRetryCircuitAndTimeoutWrap();
        IAsyncPolicy<HttpResponseMessage> WrapPolicies(params IAsyncPolicy<HttpResponseMessage>[] policies);
        IAsyncPolicy<HttpResponseMessage> RetryPolicy(int retryCount);
        IAsyncPolicy<HttpResponseMessage> TimeoutPolicy(int secondsToWaitForResponse);
        IAsyncPolicy<HttpResponseMessage> InMemoryCachePolicy(IMemoryCache memoryCache, TimeSpan ttl);
    }
}