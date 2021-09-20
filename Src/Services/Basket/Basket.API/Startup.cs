using Basket.API.GrpcServices;
using Basket.API.Helpers;
using Basket.API.Repositories;
using Basket.API.Services;
using Discount.Grpc.Protos;
using Grpc.Core;
using IdentityServer4.AccessTokenValidation;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Basket.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        //public IServiceProvider ServiceProvider { get; set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
             .AddJwtBearer(options =>
             {
                 options.Authority = "https://localhost:44318";
                 options.Audience = "basketapi";
             });

            // Redis configuration
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = Configuration.GetValue<string>("CacheSettings:ConnectionString");
            });

            services.AddScoped<IBasketRepository, BasketRepository>();
            services.AddTransient<DiscountGrpcService>();
            services.AddTransient<GrpcChannelHelper>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddAutoMapper(typeof(Startup));

            services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(o =>
            {
                o.Address = new Uri(Configuration["GrpcSettings:DiscountUrl"]);
            });
            //.ConfigureChannel(async o =>
            //{
            //    // Build the intermediate service provider
            //    var sp = services.BuildServiceProvider();
            //    //var tokenService = ServiceProvider.GetService<ITokenService>();
            //    var grpcChannelHelper = sp.GetService<GrpcChannelHelper>();
            //    await grpcChannelHelper.CreateAuthorizedChannel();

            //    //var accessToken = await tokenService.GetAccessTokenForDownstreamServices();

            //    //var credentials = CallCredentials.FromInterceptor((context, metadata) =>
            //    //{
            //    //    if (!string.IsNullOrEmpty(accessToken))
            //    //    {
            //    //        metadata.Add("Authorization", $"Bearer {accessToken}");
            //    //    }
            //    //    return Task.CompletedTask;
            //    //});

            //    //o.Credentials = ChannelCredentials.Create(new SslCredentials(), credentials);
            //    o.Credentials = ChannelCredentials.Insecure; // use to allow HTTP calls
            //});

            

            // MassTransit-RabbitMQ Configuration
            services.AddMassTransit(config =>
            {
                config.UsingRabbitMq((ctx, config) => 
                { 
                    config.Host(Configuration["EventBusSettings:HostAddress"]);
                });
            });
            services.AddMassTransitHostedService();

            // Apply to all controllers authorization policy which requires all users to be authorized before executing actions
            var requiredAuthenticatedUserPolicy = new AuthorizationPolicyBuilder()
                .RequireAuthenticatedUser()
                .Build();

            services.AddControllers(configure =>
            {
                configure.Filters.Add(new AuthorizeFilter(requiredAuthenticatedUserPolicy));
            });

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "Basket.API", Version = "v1" });
                c.CustomSchemaIds(x => x.FullName);
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"bearer {token}\""
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement()
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            },
                            Scheme = "oauth2",
                            Name = "Bearer",
                            In = ParameterLocation.Header,

                        },
                        new List<string>()
                    }
                });
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket.API v1"));

            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
