using IdentityServer4.Models;
using IdentityServer4.Validation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Shopping.IDP.Services
{
    /// <summary>
    /// Custom grant validator
    /// </summary>
    public class TokenExchangeExtensionGrantValidator : IExtensionGrantValidator
    {
        public string GrantType => "urn:ietf:params:oauth:grant-type:token-exchange";
        private string _accessToken => "urn:ietf:params:oauth:token-type:access_token";

        private readonly ITokenValidator _tokenValidator;

        public TokenExchangeExtensionGrantValidator(ITokenValidator tokenValidator)
        {
            _tokenValidator = tokenValidator;
        }

        public async Task ValidateAsync(ExtensionGrantValidationContext context)
        {
            var requestedGrant = context.Request.Raw.Get("grant_type");
            if(string.IsNullOrWhiteSpace(requestedGrant) || requestedGrant != GrantType)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Invalid grant.");
                return;
            }

            var subjectToken = context.Request.Raw.Get("subject_token");
            if(string.IsNullOrWhiteSpace(subjectToken))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Subject token missing.");
                return;
            }

            var subjectTokenType = context.Request.Raw.Get("subject_token_type");
            if (string.IsNullOrWhiteSpace(subjectTokenType))
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Subject token type missing.");
                return;
            }

            // Use the subject token type to know how to validate it.
            // Must be equal as access token in our case
            if(subjectTokenType != _accessToken)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Subject token type invalid.");
                return;
            }

            var result = await _tokenValidator.ValidateAccessTokenAsync(subjectToken);
            if(result.IsError)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidGrant, "Subject token invalid.");
                return;
            }

            // if our token is valid it contains set of claims
            // get the subject from the access token
            var subjectClaim = result.Claims.FirstOrDefault(c => c.Type == "sub");
            if(subjectClaim == null)
            {
                context.Result = new GrantValidationResult(TokenRequestErrors.InvalidRequest, "Subject token must contain sub value.");
                return;
            }

            // we can add additional claims check if that demands our use case

            // ... potential authorization checks

            // ... potential claims transformation

            // This will result in new access token
            context.Result = new GrantValidationResult(
                subject: subjectClaim.Value,
                authenticationMethod: "access_token",
                claims: result.Claims
                );

            return;
        }
    }
}
