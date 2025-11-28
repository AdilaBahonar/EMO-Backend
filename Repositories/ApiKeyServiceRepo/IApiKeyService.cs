using Microsoft.Extensions.Configuration;

namespace P3AHR.Repositories.ApiKeyServiceRepo
{
    public interface IApiKeyService
    {
        public bool IsValid(string apiKey);
    }
}
