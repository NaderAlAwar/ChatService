using System;
using System.Net.Http;
using ChatService;
using ChatService.Client;
using Microsoft.AspNetCore.TestHost;

namespace ChatServiceFunctionalTests
{
    public static class TestUtils
    {
        public static Uri GetServiceUri()
        {
            string serviceUri =
                Environment.GetEnvironmentVariable("ChatServiceUri");
            if (string.IsNullOrWhiteSpace(serviceUri))
            {
                return null;
            }

            return new Uri(serviceUri);
        }

        public static ChatServiceClient CreateTestServerAndClient()
        {
            var serviceUri = GetServiceUri();

            // we can see this in VSTS to ensure that the deployment verification tests actually ran against the deployment
            Console.WriteLine($"Test Service Uri is {serviceUri}");

            if (serviceUri == null)
            {
                var server = new TestServer(WebServer.CreateWebHostBuilder());
                return new ChatServiceClient(server.CreateClient());
            }

            var httpClient = new HttpClient {BaseAddress = serviceUri};
            return new ChatServiceClient(httpClient);
        }

    }
}
