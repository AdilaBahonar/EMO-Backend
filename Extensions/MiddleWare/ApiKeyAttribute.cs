using Microsoft.AspNetCore.Mvc;

namespace P3AHR.Extensions.MiddleWare
{
    public class ApiKeyAttribute : ServiceFilterAttribute
    {
        public ApiKeyAttribute()
            : base(typeof(UserAuthorizeAttribute))
        {
        }
    }
}
