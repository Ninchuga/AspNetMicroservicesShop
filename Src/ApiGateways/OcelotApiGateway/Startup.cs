using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using Ocelot.Cache.CacheManager;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.IdentityModel.Tokens.Jwt;
using OcelotApiGateway.DelegatingHandlers;

namespace OcelotApiGateway
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // clear Microsoft changed claim names from dictionary and preserve original ones
            // e.g. Microsoft stack renames the 'sub' claim name to http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier
            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();

            // authentication scheme key that Ocelot takes when firing a request to a route. 
            // If there is no key associated Ocelot will not start up
            // This key is set to each route that we want to secure
            // If the downstream service doesn't receive this scheme with Authority and Audience it will not be accessible (unauthorized)
            var authenticationScheme = "ShoppingGatewayAuthenticationScheme";

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
             .AddJwtBearer(authenticationScheme, options =>
             {
                 options.Authority = "https://localhost:44318";
                 options.Audience = "shoppinggateway";
             });

            services.AddHttpClient();

            services.AddScoped<CatalogApiTokenExchangeDelegatingHandler>();

            services.AddOcelot()
                .AddDelegatingHandler<CatalogApiTokenExchangeDelegatingHandler>()
                .AddDelegatingHandler<BasketApiTokenExchangeDelegatingHandler>();
                //.AddCacheManager(settings => settings.WithDictionaryHandle());
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public async void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            // Ocelot will not pass request to any middleware so we can delete anything after it
            // Ocelot will handle the request all by itself
            await app.UseOcelot();
        }
    }
}
