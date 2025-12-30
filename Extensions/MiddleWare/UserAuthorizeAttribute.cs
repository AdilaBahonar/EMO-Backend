using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using P3AHR.Repositories.ApiKeyServiceRepo;
using P3AHR.Models.DTOs.UserDTOs;

namespace P3AHR.Extensions.MiddleWare
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class UserAuthorizeAttribute : Attribute, IAuthorizationFilter
    {
        private readonly IConfiguration configuration;
        private string ApiKeyHeaderName = "";
        private readonly IApiKeyService apiKeyValidator;
        public UserAuthorizeAttribute(IApiKeyService apiKeyValidator, IConfiguration configuration)
        {
            this.apiKeyValidator = apiKeyValidator;
            this.configuration = configuration;
            ApiKeyHeaderName = configuration.GetSection("Api-Key:Header_key").Value ?? "";
        }
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            string apiKey = context.HttpContext.Request.Headers[ApiKeyHeaderName];
            if (!apiKeyValidator.IsValid(apiKey))
            {
                var allowAnonymous = context.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any();
                if (allowAnonymous)
                    return;
                var account = context.HttpContext.Items["User"] as UserResponseDTO;
                if (account == null)
                {
                    context.Result = new UnauthorizedObjectResult(StatusCodes.Status401Unauthorized);
                }
            }
            else
            {
                return;
            }
        }
    }
}
