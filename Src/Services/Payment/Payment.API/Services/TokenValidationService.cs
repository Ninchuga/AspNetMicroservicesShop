using IdentityModel;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Payment.API.Services
{
    public class TokenValidationService : ITokenValidationService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _configuration;
        private readonly ILogger<TokenValidationService> _logger;

        public TokenValidationService(HttpClient httpClient, IConfiguration configuration, ILogger<TokenValidationService> logger)
        {
            _httpClient = httpClient;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task<bool> ValidateTokenAsync(string tokenToValidate, DateTime messageSentTime)
        {
            var discoveryDocumentResponse = await _httpClient.GetDiscoveryDocumentAsync(_configuration["IdentityProviderSettings:IdentityServiceUrl"]);
            if(discoveryDocumentResponse.IsError)
            {
                _logger.LogError(discoveryDocumentResponse.Error);
                return false;
            }

            try
            {
                // because discoveryDocumentResponse.KeySet.Keys are not of the type that can assigned to TokenValidationParameters.IssuerSigningKeys
                // we need to map them to RsaSecurityKey
                var issuerSigningKeys = new List<SecurityKey>();
                foreach (var webKey in discoveryDocumentResponse.KeySet.Keys)
                {
                    var e = Base64Url.Decode(webKey.E);
                    var n = Base64Url.Decode(webKey.N);

                    var key = new RsaSecurityKey(new RSAParameters { Exponent = e, Modulus = n })
                    {
                        KeyId = webKey.Kid
                    };

                    issuerSigningKeys.Add(key);
                }

                // It can happen that the messages are not consumed for days and in that case AccessToken in the message is no longer valid -> token expired
                // One way of dealing with this is to avoid checking token lifetime with ValidateLifetime = false
                // Second approach is to get a refresh token from IdentityProvider back channel once message arrives in OrderApi from the mesage bus (complex approach)
                // Third approach is to check whether the token was expired at the moment the message was received by the bus
                var tokenValidationParameters = new TokenValidationParameters()
                {
                    ValidAudience = "paymentapi",
                    ValidIssuer = _configuration["IdentityProviderSettings:IdentityServiceUrl"], // identity service url
                    IssuerSigningKeys = issuerSigningKeys,
                    //ValidateLifetime = false // this is one way of skipping token lifetime validation
                    LifetimeValidator = (notBefore, expires, securityToken, tokenValidationParameters) =>
                    {
                        // we want to return true when the 'expires' value is greater than 'receivedAt' time
                        return expires.Value.ToUniversalTime() > messageSentTime.ToUniversalTime();
                    }
                };

                new JwtSecurityTokenHandler().ValidateToken(tokenToValidate, tokenValidationParameters, out var rawValidatedToken);

                return true;
            }
            catch(SecurityTokenValidationException ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
                return false;
            }
        }
    }
}
