using Basket.API;
using Basket.API.Repositories;
using Basket.API.Services.Basket;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;
using System;
using System.IO;
using System.Linq;

namespace Shopping.IntegrationTests.Utility.Basket
{
    public class BasketFixture : IDisposable
    {
        private IConfigurationRoot _configuration;
        private IServiceScopeFactory _scopeFactory;

        public BasketFixture()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("Basket/appsettings.json", optional: false)
                .AddEnvironmentVariables();

            _configuration = builder.Build();

            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(_configuration);

            var startup = new Startup(_configuration);
            startup.ConfigureServices(services);

            _scopeFactory = services.BuildServiceProvider().GetService<IServiceScopeFactory>();
        }

        public IBasketRepository GetBasketRepository()
        {
            using var scope = _scopeFactory.CreateScope();

            var repo = scope.ServiceProvider.GetService<IBasketRepository>();

            return repo;
        }

        public ILogger<BasketService> GetBasketServiceLogger()
        {
            using var scope = _scopeFactory.CreateScope();

            var loggerFactory = scope.ServiceProvider.GetService<ILoggerFactory>();

            return loggerFactory.CreateLogger<BasketService>();
        }

        public void ResetRedisCache()
        {
            using var scope = _scopeFactory.CreateScope();

            var redisServer = CreateRedisServerCallBack();

            redisServer.FlushDatabase();
        }

        public IDistributedCache GetDistributedCache()
        {
            using var scope = _scopeFactory.CreateScope();

            return scope.ServiceProvider.GetRequiredService<IDistributedCache>();
        }

        public void Dispose()
        {
            var redisCache = GetDistributedCache() as RedisCache;
            redisCache.Dispose();
        }

        private IServer CreateRedisServerCallBack()
        {
            string connectionString = _configuration["CacheSettings:RedisConnectionString"];
            //You need allowAdmin=true to call methods .FlushDatabase and .Keys()
            //https://stackexchange.github.io/StackExchange.Redis/Basics.html
            var redis = ConnectionMultiplexer.Connect(connectionString);
            var firstEndPoint = redis.GetEndPoints().FirstOrDefault();
            if (firstEndPoint == null)
            {
                throw new ArgumentException("The endpoints collection was empty. Could not get an end point from Redis connection multiplexer.");
            }
            return redis.GetServer(firstEndPoint);
        }

        /* REDIS COMMANDS 
         * start it with Docker and run in interactive terminal
         * when you are in interactive type: redis-cli
         * to get all the keys type: keys *
         * check what's your key data type: type <key>
         * Redis supports 6 data types. You need to know what type of value that a key maps to, as for each data type, the command to retrieve it is different.
         * Here are the commands to retrieve key value:
            if value is of type string -> GET <key>
            if value is of type hash -> HGETALL <key>
            if value is of type lists -> lrange <key> <start> <end>
            if value is of type sets -> smembers <key>
            if value is of type sorted sets -> ZRANGEBYSCORE <key> <min> <max>
            if value is of type stream -> xread count <count> streams <key> <ID>.
         * to delete key: del <key>
         * to delete all keys: flushall
         */
    }
}
