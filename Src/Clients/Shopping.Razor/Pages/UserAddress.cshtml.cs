using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using IdentityModel.Client;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;

namespace Shopping.Razor.Pages
{
    public class UserAddressModel : PageModel
    {
        public string Address { get; set; }
        private readonly IHttpClientFactory _httpClientFactory;

        public UserAddressModel(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        /// <summary>
        /// Send additional request to Identity Server in order to get the 'Address' claim values for the user
        /// </summary>
        /// <returns></returns>
        public async Task OnGet()
        {
            var idpClient = _httpClientFactory.CreateClient("IDPClient");

            var metaDataResponse = await idpClient.GetDiscoveryDocumentAsync();

            if (metaDataResponse.IsError)
            {
                throw new Exception("Problem accessing the discovery endpoint.", metaDataResponse.Exception);
            }

            var accessToken = await HttpContext.GetTokenAsync(OpenIdConnectParameterNames.AccessToken);

            var userInfoResponse = await idpClient.GetUserInfoAsync(new UserInfoRequest
            {
                Address = metaDataResponse.UserInfoEndpoint,
                Token = accessToken
            });

            if (userInfoResponse.IsError)
            {
                throw new Exception("Problem accessing the UserInfo endpoint.", userInfoResponse.Exception);
            }

            Address = userInfoResponse.Claims.FirstOrDefault(c => c.Type == "address")?.Value;
        }
    }
}
