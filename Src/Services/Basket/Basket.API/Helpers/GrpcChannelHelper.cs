using Basket.API.Services;
using Grpc.Core;
using Grpc.Net.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Basket.API.Helpers
{
    public class GrpcChannelHelper
    {
        //private readonly ITokenExchangeService _tokenService;

        //public GrpcChannelHelper(ITokenExchangeService tokenService)
        //{
        //    _tokenService = tokenService;
        //}

        //public async Task CreateAuthorizedChannel()
        //{
        //    var accessToken = await _tokenService.GetAccessTokenForDownstreamService();

        //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
        //    {
        //        if (!string.IsNullOrEmpty(accessToken))
        //        {
        //            metadata.Add("Authorization", $"Bearer {accessToken}");
        //        }
        //        return Task.CompletedTask;
        //    });
        //}

        //public async Task<GrpcChannel> CreateAuthorizedChannel(string address)
        //{
        //    var accessToken = await _tokenService.GetAccessTokenForDownstreamService();

        //    var credentials = CallCredentials.FromInterceptor((context, metadata) =>
        //    {
        //        if (!string.IsNullOrEmpty(accessToken))
        //        {
        //            metadata.Add("Authorization", $"Bearer {accessToken}");
        //        }
        //        return Task.CompletedTask;
        //    });

        //    var channel = GrpcChannel.ForAddress(address, new GrpcChannelOptions
        //    {
        //        //Credentials = ChannelCredentials.Create(new SslCredentials(), credentials)
        //        Credentials = ChannelCredentials.Insecure // used for dev purposes
        //    });

        //    return channel;
        //}
    }
}
