// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.


using IdentityServer4;
using IdentityServer4.Models;
using System.Collections.Generic;

namespace Shopping.IDP
{
    public static class Config
    {
        // User basic identity info (e.g. username, password)
        // They give access to user identity data
        public static IEnumerable<IdentityResource> IdentityResources =>
            new IdentityResource[]
            { 
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(), // givenName, familyName claims will be returned
                new IdentityResources.Address(),
                new IdentityResource("roles", "Your role(s)", new List<string> { "role" })
            };

        // This will set 'aud' value in access token which is used for authentication on API
        public static IEnumerable<ApiResource> ApiResources =>
            new ApiResource[]
            {
                new ApiResource("shopping", "Shopping APIs")
                {
                    Scopes = { "shopping.fullaccess" },
                    UserClaims = new List<string> { "role" } // optionally include some user claims in access token that will be available in the API
                },
                new ApiResource("catalogapi", "Catalog API")
                {
                    Scopes = { "catalogapi.read", "catalogapi.fullaccess" },
                    UserClaims = new List<string> { "role" }
                },
                new ApiResource("discountapi", "Discount API")
                {
                    Scopes = { "discount.fullaccess" }
                },
                new ApiResource("basketapi", "Basket API")
                {
                    Scopes = { "basketapi.read", "basketapi.fullaccess"},
                    UserClaims = new List<string> { "role" }
                }
            };

        // Defines API's that are going to be used from the Client
        // They give access to API's
        public static IEnumerable<ApiScope> ApiScopes =>
            new ApiScope[]
            { 
                new ApiScope("shopping.fullaccess", "Shopping APIs Full Access"),
                new ApiScope("catalogapi.fullaccess", "Catalog API Full Access"),
                new ApiScope("catalogapi.read", "Catalog API Read Operations"),
                new ApiScope("basketapi.read", "Basket API Read Operations"),
                new ApiScope("basketapi.fullaccess", "Basket API Full Access"),
                new ApiScope("discount.fullaccess", "Discount API Full Access")
                
            };

        // This is defined in Client applications (e.g. MVC client app)
        public static IEnumerable<Client> Clients =>
            new Client[] 
            { 
                // machine to machine client credentials flow without UI interaction
                // no username and password provided by User
                new Client
                {
                    ClientName = "Shopping Machine 2 Machine Client",
                    ClientId = "shoppingm2m",
                    ClientSecrets =
                    {
                        new Secret("m2msecret".Sha256())
                    },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes =
                    {
                        "catalogapi.fullaccess"
                    }
                },
                new Client
                {
                    ClientName = "Shopping Web App Interactive Client",
                    ClientId = "shopping_web_client_interactive",
                    ClientSecrets = { new Secret("ce766e16-df99-411d-8d31-0f5bbc6b8eba".Sha256()) },
                    AllowedGrantTypes = GrantTypes.Code, // code is delivered to browser via uri redirection
                    RedirectUris = { "https://localhost:4999/signin-oidc" },
                    PostLogoutRedirectUris = { "https://localhost:4999/signout-callback-oidc" },
                    AllowedScopes = { "openid", "profile", "shoppingbasket.fullaccess" }
                },
                // Authorization code flow Client
                // When we identify with username and password on behalf of the user
                // identity (id_token) and access token both are passed to the client, MVC app in this case
                // This client combines first and second Client into one client
                new Client
                {
                    ClientName = "Shopping Web App Client",
                    ClientId = "shopping_web_client",
                    AllowedGrantTypes = GrantTypes.CodeAndClientCredentials, 
                    RedirectUris = new List<string>()
                    {
                        // host address of our web application (MVC client)
                        "https://localhost:4999/signin-oidc"
                    },
                    PostLogoutRedirectUris = new List<string>()
                    {
                        "https://localhost:4999/signout-callback-oidc"
                    },
                    AllowedScopes =
                    {
                        // which scopes are allowed to be requested from this client
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        IdentityServerConstants.StandardScopes.Address,
                        "roles",
                        "catalogapi.fullaccess",
                        "catalogapi.read",
                        "basketapi.read",
                        "basketapi.fullaccess"
                    },
                    ClientSecrets =
                    {
                        // it's used for client authentication to allow client app to call token endpoint
                        new Secret("authorizationInteractiveSecret".Sha256())
                    }
                },
                // Client for downstream services without user interaction (no login page with username and password)
                new Client
                {
                    ClientId = "shoppingbasketdownastreamtokenexchangeclient",
                    ClientName = "Shopping Basket Token Exchange Client",
                    AllowedGrantTypes = new[] { "urn:ietf:params:oauth:grant-type:token-exchange" },
                    ClientSecrets = { new Secret("downstreamtokenexchangesecret".Sha256()) },
                    AllowedScopes =
                    {
                        IdentityServerConstants.StandardScopes.OpenId,
                        IdentityServerConstants.StandardScopes.Profile,
                        "discount.fullaccess"
                    }
                }
            };
    }
}