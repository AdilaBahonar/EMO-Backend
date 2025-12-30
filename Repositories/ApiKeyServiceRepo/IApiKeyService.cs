using Microsoft.Extensions.Configuration;

namespace EMO.Repositories.ApiKeyServiceRepo
{
    public interface IApiKeyService
    {
        public bool IsValid(string apiKey);
    }
}
