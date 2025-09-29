namespace MVCWebInvite.Utils
{
    public class AuthorizedClientProvider : IAuthorizedClientProvider
    {
        private readonly IHttpContextAccessor _contextAccessor;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<AuthorizedClientProvider> _logger;

        public AuthorizedClientProvider(IHttpContextAccessor contextAccessor, IHttpClientFactory httpClientFactory, ILogger<AuthorizedClientProvider> logger)
        {
            _contextAccessor = contextAccessor;
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public HttpClient GetClient()
        {
            var token = _contextAccessor.HttpContext?.Session.GetString("JWToken");
            _logger.LogInformation("GetClient. Token: {HasToken}", string.IsNullOrEmpty(token)? "null or empty" : "token");
            if (string.IsNullOrEmpty(token)) throw new InvalidCastException("JWT token is missing in session");
            if (JwtUtils.IsJwtExpired(token)) throw new InvalidCastException("JWT token has expired");
            var client = _httpClientFactory.CreateClient("BackendAPI");
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            return client;
        }
    }
}
