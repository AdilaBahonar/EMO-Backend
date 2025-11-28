using Microsoft.AspNetCore.Authorization;
using P3AHR.Repositories.JWTUtilsRepo;

namespace P3AHR.Extensions.MiddleWare
{
    public class JWTMiddleWare
    {
        private readonly RequestDelegate _next;
        public JWTMiddleWare(RequestDelegate next)
        {
            _next = next;
        }
        /* public async Task Invoke(HttpContext context, IJWTUtils jwtUtils)
         {
             var endpoint = context.GetEndpoint();
             if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is object)
             {
                 await _next(context);
                 return;
             }
             else
             {
                 var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
                 if (token != null)
                 {
                     var response = await jwtUtils.ValidateToken(token);
                     if (response.success == true)
                     {
                         context.Items["User"] = response.data;
                     }
                 }
                 await _next(context);
             }

         }*/

        public async Task Invoke(HttpContext context, IJWTUtils jwtUtils)
        {
            var endpoint = context.GetEndpoint();

            // Allow anonymous access
            if (endpoint?.Metadata?.GetMetadata<IAllowAnonymous>() is object)
            {
                await _next(context);
                return;
            }

            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (token != null)
            {
                var response = await jwtUtils.ValidateToken(token);

                if (response.success)
                {
                    context.Items["User"] = response.data;
                    await _next(context);
                    return;
                }
                else if (response.remarks == "TokenExpired")
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token has expired.");
                    return;
                }
                else if (response.remarks == "TokenInvalid")
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Token is invalid.");
                    return;
                }
                else if (response.remarks == "UserNotFound")
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("User not found for token.");
                    return;
                }
            }

            // No token provided
            await _next(context);
        }
    }
}
