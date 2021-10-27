using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Shopping.Correlation;
using Shopping.HealthChecks;
using Shopping.Logging;
using Shopping.MVC.Extensions;
using Shopping.MVC.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Threading.Tasks;

namespace Shopping.MVC
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // clear Microsoft changed claim names from dictionary and preserve original ones
            // e.g. Microsoft stack renames the 'sub' claim name to http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHealthChecks();

            services.AddRazorPages(options => 
            {
                options.Conventions.AuthorizePage("/Catalog");
                options.Conventions.AuthorizePage("/Basket/GetBasket");
                options.Conventions.AuthorizePage("/OrderItem", "PremiumUserRolePolicy");
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("PremiumUserRolePolicy", policy => policy.RequireRole("PremiumUser"));
            });

            // Used for refresh token
            services.AddAccessTokenManagement();

            //services.AddCors();

            // Response Type Values
            // code -> Authorization Code flow
            // id_token -> Implicit flow
            // id_token token -> Implicit flow where token is an access token
            // code id_token -> Hybrid flow
            // code token -> Hybrid flow
            // code id_token token -> Hybrid flow
            services.AddAuthentication(options =>
            {
                options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
            })
            .AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
            {
                options.AccessDeniedPath = "/Authorization/AccessDenied";
            })
            .AddOpenIdConnect(OpenIdConnectDefaults.AuthenticationScheme, options =>
            {
                options.MetadataAddress = $"{Configuration["IdentityProviderSettings:IdentityServiceUrl"]}/.well-known/openid-configuration";
                options.Configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration
                {
                    JwksUri = $"{Configuration["IdentityProviderSettings:IdentityServiceUrl"]}/.well-known/openid-configuration/jwks",
                    AuthorizationEndpoint = $"{Configuration["IdentityProviderSettings:IdentityServiceUrl"]}/authorization_endpoint",
                    Issuer = "my_auth"
                };
                options.RequireHttpsMetadata = false; // to disable HTTPS when calling identity authority
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // this ensures that valid authentication will be stored in cookie
                options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
                options.ClientId = "shopping_web_client";
                options.ClientSecret = "authorizationInteractiveSecret";
                options.ResponseType = "code";
                options.Scope.Add("roles");
                options.Scope.Add("shoppinggateway.fullaccess");
                options.Scope.Add("shoppingaggregator.fullaccess");
                options.Scope.Add("offline_access"); // scope for refresh token
                //options.Scope.Add("catalogapi.fullaccess");
                //options.Scope.Add("basketapi.fullaccess");
                //options.CallbackPath.
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    // Intercept the redirection so the browser navigates to the right URL in your host
                    context.ProtocolMessage.IssuerAddress = Configuration["IdentityProviderRedirectUris:AuthorizationAddress"]; //"https://sso-mybest.domain.com/connect/authorize";
                    context.ProtocolMessage.RedirectUri = Configuration["IdentityProviderRedirectUris:SigninRedirectUri"]; //"https://mybest.domain.com/signin-oidc";
                    context.ProtocolMessage.AuthorizationEndpoint = $"{Configuration["IdentityProviderSettings:IdentityServiceUrl"]}/authorization_endpoint";
                    context.ProtocolMessage.BuildRedirectUrl();
                    //context.ProtocolMessage.TokenEndpoint
                    return Task.CompletedTask;
                };
                //options.Events.OnRedirectToIdentityProviderForSignOut = context =>
                //{
                //    context.ProtocolMessage.PostLogoutRedirectUri = Configuration["IdentityProviderRedirectUris:SignoutRedirectUri"];
                //    return Task.CompletedTask;
                //};
                options.ClaimActions.DeleteClaim("sid");
                options.ClaimActions.DeleteClaim("idp");
                options.ClaimActions.DeleteClaim("s_hash");
                options.ClaimActions.DeleteClaim("auth_time");
                options.ClaimActions.MapUniqueJsonKey("role", "role"); // since this claim is not set by default we need to add it manually on client
                options.ClaimActions.MapUniqueJsonKey("preferred_username", "preferred_username");
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.GivenName,
                    RoleClaimType = JwtClaimTypes.Role,
                    //ValidateIssuer = false,
                    //Validate
                };
            });

            services.AddHttpContextAccessor();
            services.AddTransient<LoggingDelegatingHandler>();
            services.AddTransient<CorrelationIdDelegatingHandler>();

            services.AddHttpClient<CatalogService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ApiSettings:Catalog:CatalogGatewayUrl"]);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddUserAccessTokenHandler(); // used for refresh token flow
            //.AddHttpMessageHandler<BearerTokenHandler>(); // call access token middleware and get/set access token to request message
            //        .AddPolicyHandler(GetRetryPolicy())
            //.AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<BasketService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ApiSettings:Basket:BasketGatewayUrl"]);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddUserAccessTokenHandler(); // used for refresh token flow
            //.AddHttpMessageHandler<BearerTokenHandler>(); // call access token middleware and get/set access token to request message;

            services.AddHttpClient<OrderService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ApiSettings:Order:OrderGatewayUrl"]);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddUserAccessTokenHandler(); // used for refresh token flow
            //.AddHttpMessageHandler<BearerTokenHandler>(); // call access token middleware and get/set access token to request message;

            services.AddHttpClient<ShoppingService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ApiSettings:Aggregators:ShoppingAggregatorUrl"]);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            })
            .AddHttpMessageHandler<CorrelationIdDelegatingHandler>()
            .AddHttpMessageHandler<LoggingDelegatingHandler>()
            .AddUserAccessTokenHandler(); // used for refresh token flow
            //.AddHttpMessageHandler<BearerTokenHandler>(); // call access token middleware and get/set access token to request message;

            // Used to get additional UserInfo (e.g. address) from IdentityServer
            services.AddHttpClient("IDPClient", client =>
            {
                client.BaseAddress = new Uri("https://localhost:44318");
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                IdentityModelEventSource.ShowPII = true;
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            ServicePointManager.Expect100Continue = true;
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
            | SecurityProtocolType.Tls11
            | SecurityProtocolType.Tls12
            | SecurityProtocolType.Tls13;

            app.UseMiddleware<ErrorHandlerMiddleware>();
            app.UseMiddleware<CorrelationIdMiddleware>();

            //app.UseCors(
            //    builder => builder
            //        .AllowAnyHeader()
            //        .AllowAnyMethod()
            //        .AllowAnyOrigin());
            //        //.WithOrigins("http://localhost:32773"));

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapDefaultHealthChecks();
                endpoints.MapRazorPages();
            });
        }
    }
}
