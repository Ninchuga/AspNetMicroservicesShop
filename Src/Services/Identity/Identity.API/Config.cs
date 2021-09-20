// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4.Models;
using System.Collections.Generic;

namespace Identity.API
{
    public static class Config
    {
        public static IEnumerable<IdentityResource> IdentityResources =>
                   new IdentityResource[]
                   {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                   };

        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            {
                new ApiScope("scope1"),
                new ApiScope("scope2"),
                new ApiScope("shoppingAggregator", "Shopping Aggregator")
            };

        // There are 3 code flows that we can use for authentication and they are all good for server side clients (e.g. MVC client app)
        // 1. Authorization flow
        // 2. Implicit flow (doesn't support refresh tokens)
        // 3. Hybrid flow
        public static IEnumerable<Client> Clients =>
            new Client[]
            {
                // m2m client credentials flow client
                new Client
                {
                    ClientId = "shoppingClient",
                    ClientName = "Client Credentials Client",

                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    ClientSecrets = { new Secret("511536EF-F270-4058-80CA-1C89C192F69A".Sha256()) },

                    AllowedScopes = { "scope1", "shoppingAggregator" }
                },

                // interactive client using code flow + pkce
                new Client
                {
                    ClientId = "shopping_web_client", //"interactive",
                    ClientName = "Shopping Web App",
                    AllowRememberConsent = false,
                    ClientSecrets = { new Secret("49C1A7E1-0C79-4A89-A3D6-A37998FB86B0".Sha256()) },

                    AllowedGrantTypes = GrantTypes.Code,

                    RedirectUris = { "https://localhost:4999/signin-oidc" },
                    FrontChannelLogoutUri = "https://localhost:4999/signout-oidc",
                    PostLogoutRedirectUris = { "https://localhost:4999/signout-callback-oidc" },

                    AllowOfflineAccess = true,
                    AllowedScopes = { "openid", "profile", "scope2" }
                },
            };
    }
}