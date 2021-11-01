using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Shopping.Correlation;
using Shopping.HealthChecks;
using Shopping.Logging;
using Shopping.MVC.Extensions;
using Shopping.MVC.Services;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
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
                // works with docker but not with localhost
                //var openIdConnectConfiguration = OpenIdConnectConfiguration.Create("{'issuer':'https://localhost:8021','jwks_uri':'https://localhost:8021/.well-known/openid-configuration/jwks','authorization_endpoint':'https://localhost:8021/connect/authorize','token_endpoint':'https://localhost:8021/connect/token','userinfo_endpoint':'https://localhost:8021/connect/userinfo','end_session_endpoint':'https://localhost:8021/connect/endsession','check_session_iframe':'https://localhost:8021/connect/checksession','revocation_endpoint':'https://localhost:8021/connect/revocation','introspection_endpoint':'https://localhost:8021/connect/introspect','device_authorization_endpoint':'https://localhost:8021/connect/deviceauthorization','frontchannel_logout_supported':true,'frontchannel_logout_session_supported':true,'backchannel_logout_supported':true,'backchannel_logout_session_supported':true,'scopes_supported':['roles','address','profile','openid','shoppingaggregator.fullaccess','orderapi.write','orderapi.read','shoppinggateway.fullaccess','discount.fullaccess','basketapi.fullaccess','basketapi.read','catalogapi.read','catalogapi.fullaccess','offline_access'],'claims_supported':['role','address','family_name','given_name','middle_name','nickname','preferred_username','profile','name','picture','gender','birthdate','zoneinfo','locale','updated_at','website','sub'],'grant_types_supported':['authorization_code','client_credentials','refresh_token','implicit','password','urn:ietf:params:oauth:grant-type:device_code','urn:ietf:params:oauth:grant-type:token-exchange'],'response_types_supported':['code','token','id_token','id_token token','code id_token','code token','code id_token token'],'response_modes_supported':['form_post','query','fragment'],'token_endpoint_auth_methods_supported':['client_secret_basic','client_secret_post'],'id_token_signing_alg_values_supported':['RS256'],'subject_types_supported':['public'],'code_challenge_methods_supported':['plain','S256'],'request_parameter_supported':true}");
                //options.MetadataAddress = $"{Configuration["IdentityProviderSettings:IdentityServiceUrl"]}/.well-known/openid-configuration";
                //options.Configuration = new Microsoft.IdentityModel.Protocols.OpenIdConnect.OpenIdConnectConfiguration
                //{
                //    JwksUri = $"{Configuration["IdentityProviderSettings:IdentityServiceUrl"]}/.well-known/openid-configuration/jwks",
                //    AuthorizationEndpoint = $"{Configuration["IdentityProviderSettings:IdentityServiceUrl"]}/authorization_endpoint",
                //    Issuer = $"{Configuration["IdentityProviderSettings:IdentityServiceUrl"]}" //"my_auth"
                //};
                //options.Configuration = openIdConnectConfiguration;
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
                //options.SignedOutRedirectUri = callBackUrl.ToString();
                //options.CallbackPath = "/signin-oidc";
                options.Events.OnRedirectToIdentityProvider = context =>
                {
                    // Intercept the redirection so the browser navigates to the right URL in your host
                    //context.ProtocolMessage.IssuerAddress = Configuration["IdentityProviderRedirectUris:AuthorizationAddress"]; //"https://sso-mybest.domain.com/connect/authorize";
                    context.ProtocolMessage.RedirectUri = Configuration["IdentityProviderRedirectUris:SigninRedirectUri"]; //"https://mybest.domain.com/signin-oidc";
                    //context.ProtocolMessage.AuthorizationEndpoint = $"{Configuration["IdentityProviderSettings:IdentityServiceUrl"]}/authorization_endpoint";
                    //context.ProtocolMessage.BuildRedirectUrl();
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
                    RoleClaimType = JwtClaimTypes.Role
                };
            });

            services.AddDataProtection()
                    //.PersistKeysToFileSystem(new DirectoryInfo(@"\\server\share\directory\"));
                    .PersistKeysToFileSystem(new DirectoryInfo(@"D:\temp-keys\"));
            //.PersistKeysToDbContext<DbContext>() // he preceding code stores the keys in the configured database. 
            // The database context being used must implement IDataProtectionKeyContext. IDataProtectionKeyContext exposes the property DataProtectionKeys
            // https://docs.microsoft.com/en-us/aspnet/core/security/data-protection/configuration/overview?view=aspnetcore-3.1

            services.Configure<ForwardedHeadersOptions>(options =>
            {
                options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
                //options.KnownNetworks.Add(new IPNetwork(IPAddress.Parse("172.30.32.1"), 24));
            });

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.None;
                options.Secure = CookieSecurePolicy.Always;
                //options.HandleSameSiteCookieCompatibility();
            });

            //services.Configure<CookiePolicyOptions>(options =>
            //{
            //    options.MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
            //    options.OnAppendCookie = cookieContext =>
            //        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            //    options.OnDeleteCookie = cookieContext =>
            //        CheckSameSite(cookieContext.Context, cookieContext.CookieOptions);
            //});

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

            app.UseForwardedHeaders();
            app.UseCookiePolicy();
            //app.UseCookiePolicy(new CookiePolicyOptions
            //{
            //    HttpOnly = HttpOnlyPolicy.None,
            //    MinimumSameSitePolicy = Microsoft.AspNetCore.Http.SameSiteMode.Strict,
            //    Secure = CookieSecurePolicy.Always
            //});

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

        //private void CheckSameSite(HttpContext httpContext, CookieOptions options)
        //{
        //    if (options.SameSite == Microsoft.AspNetCore.Http.SameSiteMode.None)
        //    {
        //        var userAgent = httpContext.Request.Headers["User-Agent"].ToString();
        //        // TODO: Use your User Agent library of choice here. 
        //        if (DisallowsSameSiteNone(userAgent))
        //        {
        //            options.SameSite = Microsoft.AspNetCore.Http.SameSiteMode.Unspecified;
        //        }
        //    }
        //}

        //public bool DisallowsSameSiteNone(string userAgent)
        //{
        //    // Check if a null or empty string has been passed in, since this
        //    // will cause further interrogation of the useragent to fail.
        //    if (String.IsNullOrWhiteSpace(userAgent))
        //        return false;

        //    // Cover all iOS based browsers here. This includes:
        //    // - Safari on iOS 12 for iPhone, iPod Touch, iPad
        //    // - WkWebview on iOS 12 for iPhone, iPod Touch, iPad
        //    // - Chrome on iOS 12 for iPhone, iPod Touch, iPad
        //    // All of which are broken by SameSite=None, because they use the iOS networking
        //    // stack.
        //    if (userAgent.Contains("CPU iPhone OS 12") ||
        //        userAgent.Contains("iPad; CPU OS 12"))
        //    {
        //        return true;
        //    }

        //    // Cover Mac OS X based browsers that use the Mac OS networking stack. 
        //    // This includes:
        //    // - Safari on Mac OS X.
        //    // This does not include:
        //    // - Chrome on Mac OS X
        //    // Because they do not use the Mac OS networking stack.
        //    if (userAgent.Contains("Macintosh; Intel Mac OS X 10_14") &&
        //        userAgent.Contains("Version/") && userAgent.Contains("Safari"))
        //    {
        //        return true;
        //    }

        //    // Cover Chrome 50-69, because some versions are broken by SameSite=None, 
        //    // and none in this range require it.
        //    // Note: this covers some pre-Chromium Edge versions, 
        //    // but pre-Chromium Edge does not require SameSite=None.
        //    if (userAgent.Contains("Chrome/5") || userAgent.Contains("Chrome/6"))
        //    {
        //        return true;
        //    }

        //    return false;
        //}
    }
}
