using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using OcelotApiGateway.DelegatingHandlers;
using Microsoft.Extensions.Configuration;
using Shopping.HealthChecks;
using Ocelot.Provider.Polly;

namespace OcelotApiGateway
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            // clear Microsoft changed claim names from dictionary and preserve original ones
            // e.g. Microsoft stack renames the 'sub' claim name to http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Used for storing access tokens in the cache in a delegating handlers
            services.AddAccessTokenManagement();

            services.AddHealthChecks();

            services.AddCors(options =>
            {
                options.AddPolicy("default", policy =>
                {
                    policy.WithOrigins(Configuration["WebClientUrls:Angular"])
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials();
                });
            });

            // authentication scheme key that Ocelot takes when firing a request to a route. 
            // If there is no key associated Ocelot will not start up
            // This key is set to each route that we want to secure
            // If the downstream service doesn't receive this scheme with Authority and Audience it will not be accessible (unauthorized)
            var authenticationScheme = "ShoppingGatewayAuthenticationScheme";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(authenticationScheme, options =>
             {
                 options.Authority = Configuration["IdentityProviderSettings:IdentityServiceUrl"];
                 options.Audience = "shoppinggateway";
                 options.RequireHttpsMetadata = false;
             });

            services.AddHttpClient();

            services.AddOcelot()
                .AddDelegatingHandler<TokenExchangeDelegatingHandler>()
                .AddPolly();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("default");

            app.UseRouting();

            app.UseEndpoints((endpoint) =>
            {
                endpoint.MapDefaultHealthChecks();
            });

            app.UseHttpsRedirection();
            // Ocelot will not pass request to any middleware so we can delete anything after it
            // Ocelot will handle the request all by itself
            await app.UseOcelot();
        }
    }
}
