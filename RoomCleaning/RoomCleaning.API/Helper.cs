using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace RoomCleaning.API
{
    class Helper
    {
        public static GraphServiceClient GetGraphClient(IConfigurationRoot config)
        {
            var graphClient = new GraphServiceClient(new DelegateAuthenticationProvider((requestMessage) => {
                // get an access token for Graph
                var accessToken = GetAccessToken(config).Result;

                requestMessage
                    .Headers
                    .Authorization = new AuthenticationHeaderValue("bearer", accessToken);

                return Task.FromResult(0);
            }));

            return graphClient;
        }

        public static async Task<string> GetAccessToken(IConfigurationRoot config)
        {
            IConfidentialClientApplication app = ConfidentialClientApplicationBuilder.Create(config["AppId"])
              .WithClientSecret(config["AppSecret"])
              .WithAuthority($"https://login.microsoftonline.com/{config["TenantId"]}")
              .WithRedirectUri("https://daemon")
              .Build();

            string[] scopes = new string[] { "https://graph.microsoft.com/.default" };

            var result = await app.AcquireTokenForClient(scopes).ExecuteAsync();

            return result.AccessToken;
        }

        public static IConfigurationRoot GetConfig(ExecutionContext context)
        {
            return new ConfigurationBuilder()
                    .SetBasePath(context.FunctionAppDirectory)
                    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
                    .AddEnvironmentVariables()
                    .Build();
        }
    }
}
