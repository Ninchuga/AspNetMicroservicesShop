using IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Net.Http.Headers;
using Shopping.MVC.HttpHandlers;
using Shopping.MVC.Services;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
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
            services.AddRazorPages(options => 
            {
                options.Conventions.AuthorizePage("/Catalog");
                options.Conventions.AuthorizePage("/OrderItem", "PremiumUserRolePolicy");
            });
            services.AddAuthorization(options =>
            {
                options.AddPolicy("PremiumUserRolePolicy", policy => policy.RequireRole("PremiumUser"));
            });

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
                options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme; // this ensures that valid authentication will be stored in cookie
                options.Authority = "https://localhost:44318";
                options.ClientId = "shopping_web_client";
                options.ClientSecret = "authorizationInteractiveSecret";
                options.ResponseType = "code";
                options.Scope.Add("roles");
                options.Scope.Add("basketapi.fullaccess");
                //options.Scope.Add("catalogapi.fullaccess");
                options.ClaimActions.DeleteClaim("sid");
                options.ClaimActions.DeleteClaim("idp");
                options.ClaimActions.DeleteClaim("s_hash");
                options.ClaimActions.DeleteClaim("auth_time");
                options.ClaimActions.MapUniqueJsonKey("role", "role"); // since this claim is not set by default we need to add it manually on client
                options.SaveTokens = true;
                options.GetClaimsFromUserInfoEndpoint = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    NameClaimType = JwtClaimTypes.GivenName,
                    RoleClaimType = JwtClaimTypes.Role
                };
            });

            services.AddScoped<CatalogService>();
            services.AddScoped<BasketService>();
            services.AddTransient<BearerTokenHandler>();

            services.AddHttpContextAccessor();

            services.AddHttpClient<CatalogService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ApiSettings:Catalog:CatalogGatewayUrl"]);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });
            //.AddHttpMessageHandler<BearerTokenHandler>(); // call access token middleware and get/set access token to request message
            //        .AddPolicyHandler(GetRetryPolicy())
            //.AddPolicyHandler(GetCircuitBreakerPolicy());

            services.AddHttpClient<BasketService>(client =>
            {
                client.BaseAddress = new Uri(Configuration["ApiSettings:Basket:BasketGatewayUrl"]);
                client.DefaultRequestHeaders.Clear();
                client.DefaultRequestHeaders.Add(HeaderNames.Accept, "application/json");
            });
            //.AddHttpMessageHandler<BearerTokenHandler>(); // call access token middleware and get/set access token to request message;

            // Used to get addition UserInfo (e.g. address) from IdentityServer
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
            }
            else
            {
                app.UseExceptionHandler("/Error");
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            //app.UseCookiePolicy(new CookiePolicyOptions { MinimumSameSitePolicy = SameSiteMode.Strict });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapRazorPages();
            });
        }
    }
}
