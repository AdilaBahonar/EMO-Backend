using Microsoft.AspNetCore.Mvc;

namespace EMO.Extensions.MiddleWare
{
    public class ApiKeyAttribute : ServiceFilterAttribute
    {
        public ApiKeyAttribute()
            : base(typeof(UserAuthorizeAttribute))
        {
        }
    }
}
