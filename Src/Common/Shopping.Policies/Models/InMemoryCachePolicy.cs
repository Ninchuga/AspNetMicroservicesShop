using Microsoft.Extensions.Caching.Memory;
using Polly;
using System;
using System.Net.Http;

namespace Shopping.Policies.Models
{
    public class InMemoryCachePolicy : IAmPolicy
    {
        public string PolicyName { get; }
        public TimeSpan Ttl { get; }

        public InMemoryCachePolicy(TimeSpan ttl)
        {
            PolicyName = AvailablePolicies.InMemoryCachePolicy.ToString();
            Ttl = ttl;
        }

        public IAsyncPolicy<HttpResponseMessage> InMemoryCachePolicyHandler(IMemoryCache memoryCache)
        {
            Polly.Caching.Memory.MemoryCacheProvider memoryCacheProvider = new Polly.Caching.Memory.MemoryCacheProvider(memoryCache);

            return Policy.CacheAsync<HttpResponseMessage>(memoryCacheProvider, Ttl);
        }
    }
}
