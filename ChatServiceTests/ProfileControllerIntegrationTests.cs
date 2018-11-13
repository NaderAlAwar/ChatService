﻿using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using ChatService;
using ChatService.Client;
using ChatService.DataContracts;
using ChatService.Storage;
using ChatServiceTests.Utils;
using Microsoft.AspNetCore.TestHost;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceTests
{
    /// <summary>
    /// The integration tests are used to validate the full API execution end-to-end.
    /// These are usually the tests that allow us to find most issues and they tend to
    /// be less fragile because they are decoupled from implementation details (they rely only
    /// on the document API).
    /// </summary>
    [TestClass]
    [TestCategory("Integration")]
    public class ProfileControllerIntegrationTests
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
        public async Task CreateGetProfile()
        {
            var createProfileDto = new CreateProfileDto
            {
                Username = Guid.NewGuid().ToString(),
                FirstName = "Nehme",
                LastName = "Bilal"
            };

            await chatServiceClient.CreateProfile(createProfileDto);
            UserInfoDto userProfile = await chatServiceClient.GetProfile(createProfileDto.Username);

            Assert.AreEqual(createProfileDto.Username, userProfile.Username);
            Assert.AreEqual(createProfileDto.FirstName, userProfile.FirstName);
            Assert.AreEqual(createProfileDto.LastName, userProfile.LastName);
        }

        [TestMethod]
        public async Task GetNonExistingProfile()
        {
            try
            {
                await chatServiceClient.GetProfile(Guid.NewGuid().ToString());
                Assert.Fail("A ChatServiceException was expected but was not thrown");
            }
            catch (ChatServiceException e)
            {
                Assert.AreEqual(HttpStatusCode.ServiceUnavailable, e.StatusCode);
            }
        }

        [TestMethod]
        public async Task CreateDuplicateProfile()
        {
            var createProfileDto = new CreateProfileDto
            {
                Username = Guid.NewGuid().ToString(),
                FirstName = "Nehme",
                LastName = "Bilal"
            };

            await chatServiceClient.CreateProfile(createProfileDto);

            try
            {
                await chatServiceClient.CreateProfile(createProfileDto);
                Assert.Fail("A ChatServiceException was expected but was not thrown");
            }
            catch (ChatServiceException e)
            {
                Assert.AreEqual(HttpStatusCode.Conflict, e.StatusCode);
            }
        }

        [TestMethod]
        [DataRow("", "Nehme", "Bilal")]
        [DataRow(null, "Nehme", "Bilal")]
        [DataRow("nbilal", "", "Bilal")]
        [DataRow("nbilal", null, "Bilal")]
        [DataRow("nbilal", "Nehme", "")]
        [DataRow("nbilal", "Nehme", null)]
        public async Task CreateProfile_InvalidDto(string username, string firstName, string lastName)
        {
            var createProfileDto = new CreateProfileDto
            {
                Username = username,
                FirstName = firstName, 
                LastName = lastName
            };

            try
            {
                await chatServiceClient.CreateProfile(createProfileDto);
                Assert.Fail("A ChatServiceException was expected but was not thrown");
            }
            catch (ChatServiceException e)
            {
                Assert.AreEqual(HttpStatusCode.BadRequest, e.StatusCode);
            }
        }
    }
}
