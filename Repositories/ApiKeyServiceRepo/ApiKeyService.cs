namespace P3AHR.Repositories.ApiKeyServiceRepo
{
    public class ApiKeyService : IApiKeyService
    {
        private readonly IConfiguration configuration;
        public ApiKeyService(IConfiguration configuration)
        {
            this.configuration = configuration;
        }
        public bool IsValid(string apiKey)
        {
            var key = configuration.GetSection("Api-Key:Key").Value ?? "";
            if (apiKey == key)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
