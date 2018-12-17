using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using ChatService;
using ChatService.Client;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class ApplicationControllerIntegrationTests
    {
        private HttpClient httpClient;
        private TestServer server;
        private ChatServiceClient chatServiceClient;

        [TestInitialize]
        public void TestInitialize()
        {
            server = new TestServer(WebServer.CreateWebHostBuilder());
            httpClient = server.CreateClient();
            chatServiceClient = new ChatServiceClient(httpClient);
        }

        [TestMethod]
        public async Task GetVersionTest()
        {
            var response = await chatServiceClient.GetVersion();
            Assert.IsNotNull(response);
        }

        [TestMethod]
        public async Task GetEnvironmentTest()
        {
            var response = await chatServiceClient.GetEnvironment();
            Assert.IsNotNull(response);
        }
    }
}
