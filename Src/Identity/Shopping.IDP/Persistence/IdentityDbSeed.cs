using IdentityModel;
using IdentityServer4;
using IdentityServer4.EntityFramework.DbContexts;
using IdentityServer4.EntityFramework.Mappers;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Serilog;
using Shopping.IDP.Models;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace Shopping.IDP.Persistence
{
    public class IdentityDbSeed
    {
        private const string ShoppingWebClientId = "shopping_web_client";

        public static async Task SeedIdentityConfiguration(ConfigurationDbContext context, ILogger logger, IConfiguration configuration)
        {
            if (!context.Clients.Any())
            {
                foreach (var client in Config.Clients(configuration))
                {
                    context.Clients.Add(client.ToEntity());
                }
                await context.SaveChangesAsync();
            }
            else
            {
                await UpdateClientRedirectUris(context, configuration, ShoppingWebClientId);
            }

            if (!context.IdentityResources.Any())
            {
                foreach (var resource in Config.IdentityResources)
                {
                    context.IdentityResources.Add(resource.ToEntity());
                }
                await context.SaveChangesAsync();
            }

            if (!context.ApiResources.Any())
            {
                foreach (var resource in Config.ApiResources)
                {
                    context.ApiResources.Add(resource.ToEntity());
                }
                await context.SaveChangesAsync();
            }

            if (!context.ApiScopes.Any())
            {
                foreach (var scope in Config.ApiScopes)
                {
                    context.ApiScopes.Add(scope.ToEntity());
                }
                await context.SaveChangesAsync();
            }

            logger.Information("Seed database associated with context {DbContextName}", nameof(ConfigurationDbContext));
        }

        private static async Task UpdateClientRedirectUris(ConfigurationDbContext context, IConfiguration configuration, string clientId)
        {
            // when switching from localhost to docker environment redirect uri is different
            // so we need to update it in that case with uri that we pass through configuration
            var dbWebClient = context.Clients
                                     .Include(c => c.RedirectUris)
                                     .Include(c => c.PostLogoutRedirectUris)
                                     .FirstOrDefault(client => client.ClientId.Equals(clientId));
            var redirectUri = dbWebClient.RedirectUris.FirstOrDefault(cr => cr.ClientId == dbWebClient.Id);
            var postLogoutUri = dbWebClient.PostLogoutRedirectUris.FirstOrDefault(cr => cr.ClientId == dbWebClient.Id);

            var configWebClient = Config.Clients(configuration).FirstOrDefault(client => client.ClientId.Equals(clientId));
            redirectUri.RedirectUri = configWebClient.RedirectUris.ToArray()[0];
            postLogoutUri.PostLogoutRedirectUri = configWebClient.PostLogoutRedirectUris.ToArray()[0];

            context.Update(redirectUri);
            context.Update(postLogoutUri);
            await context.SaveChangesAsync();
        }

        public static async Task SeedIdentityUsers(ApplicationDbContext context, ILogger logger, UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            await AddPremiumUser(userManager, roleManager);
            await AddFreeUser(userManager, roleManager);
            logger.Information("Added default identity users to context {DbContextName}", nameof(ApplicationDbContext));
        }

        private static async Task AddPremiumUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var premiumUserRole = new IdentityRole("PremiumUser");
            if (roleManager.Roles.All(r => r.Name != premiumUserRole.Name))
            {
                await roleManager.CreateAsync(premiumUserRole);
            }

            var premiumUser = new ApplicationUser
            {
                Id = "bbf594b0-3761-4a65-b04c-eec4836d9070",
                UserName = "ninchuga",
                FirstName = "Ninoslav",
                LastName = "Djukic",
                Email = "ninomail@email.com"
            };
            var address = new
            {
                street_address = "One Hacker Way",
                locality = "Heidelberg",
                postal_code = 69118,
                country = "Germany"
            };

            if (userManager.Users.All(u => u.UserName != premiumUser.UserName))
            {
                await userManager.CreateAsync(premiumUser, "ninoOnDocker25!");
                await userManager.AddToRolesAsync(premiumUser, new[] { premiumUserRole.Name });

                var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Name, $"{premiumUser.FirstName} {premiumUser.LastName}"),
                    new Claim(JwtClaimTypes.PreferredUserName, premiumUser.UserName),
                    new Claim(JwtClaimTypes.GivenName, premiumUser.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, premiumUser.LastName),
                    new Claim(JwtClaimTypes.Email, premiumUser.Email),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://nino.com"),
                    new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json),
                    new Claim(JwtClaimTypes.Role, premiumUserRole.Name)
                };

                await userManager.AddClaimsAsync(premiumUser, claims);
            }
        }

        private static async Task AddFreeUser(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            var freeUserRole = new IdentityRole("FreeUser");
            if (roleManager.Roles.All(r => r.Name != freeUserRole.Name))
            {
                await roleManager.CreateAsync(freeUserRole);
            }

            var freeUser = new ApplicationUser
            {
                Id = "e455a3df-7fa5-47e0-8435-179b300d531f",
                UserName = "alice",
                FirstName = "Alice",
                LastName = "Smith",
                Email = "AliceSmith@email.com"
            };
            var address = new
            {
                street_address = "One Hacker Way",
                locality = "Heidelberg",
                postal_code = 69118,
                country = "Germany"
            };

            if (userManager.Users.All(u => u.UserName != freeUser.UserName))
            {
                await userManager.CreateAsync(freeUser, "aliceOnDocker25!");
                await userManager.AddToRolesAsync(freeUser, new[] { freeUserRole.Name });

                var claims = new List<Claim>
                {
                    new Claim(JwtClaimTypes.Name, $"{freeUser.FirstName} {freeUser.LastName}"),
                    new Claim(JwtClaimTypes.PreferredUserName, freeUser.UserName),
                    new Claim(JwtClaimTypes.GivenName, freeUser.FirstName),
                    new Claim(JwtClaimTypes.FamilyName, freeUser.LastName),
                    new Claim(JwtClaimTypes.Email, freeUser.Email),
                    new Claim(JwtClaimTypes.EmailVerified, "true", ClaimValueTypes.Boolean),
                    new Claim(JwtClaimTypes.WebSite, "http://nino.com"),
                    new Claim(JwtClaimTypes.Address, JsonSerializer.Serialize(address), IdentityServerConstants.ClaimValueTypes.Json),
                    new Claim(JwtClaimTypes.Role, freeUserRole.Name)
                };

                await userManager.AddClaimsAsync(freeUser, claims);
            }
        }

        
    }
}
