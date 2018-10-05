﻿using System;
using System.IO;
using System.Threading.Tasks;
using ChatService.Storage;
using ChatService.Storage.Azure;
using ChatServiceTests.Utils;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("Integration")]
    public class AzureTableProfileStoreIntegrationTests
    {
        private static string ConnectionString { get; set; }
        private AzureTableProfileStore store;
        private readonly UserProfile testProfile = new UserProfile(Guid.NewGuid().ToString(), "Nehme", "Bilal");

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            IConfiguration configuration = InitConfiguration();
            IConfiguration storageConfiguration = configuration.GetSection(nameof(AzureStorageSettings));
            AzureStorageSettings azureStorageSettings = new AzureStorageSettings();
            storageConfiguration.Bind(azureStorageSettings);
            ConnectionString = azureStorageSettings.ConnectionString;
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            var table = new AzureCloudTable(ConnectionString, "ProfileTestTable");
            await table.CreateIfNotExistsAsync();
            store = new AzureTableProfileStore(table);
            
        }

        [TestCleanup]
        public async Task TestCleanup()
        {
            await store.TryDelete(testProfile.Username);
        }

        [TestMethod]
        public async Task AddGetProfile()
        {
            await store.AddProfile(testProfile);
            var profile = await store.GetProfile(testProfile.Username);
            Assert.AreEqual(testProfile, profile);
        }

        [TestMethod]
        [ExpectedException(typeof(ProfileNotFoundException))]
        public async Task GetNonExistingProfile()
        {
            await store.GetProfile(testProfile.Username);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task GetProfile_NullUsername()
        {
            await store.GetProfile(null);
        }

        [TestMethod]
        [ExpectedException(typeof(DuplicateProfileException))]
        public async Task AddExistingProfile()
        {
            await store.AddProfile(testProfile);
            await store.AddProfile(testProfile);
        }

        [DataRow("", "Nehme", "Bilal")]
        [DataRow(null, "Nehme", "Bilal")]
        [DataRow("nbilal", "", "Bilal")]
        [DataRow("nbilal", null, "Bilal")]
        [DataRow("nbilal", "Nehme", "")]
        [DataRow("nbilal", "Nehme", null)]
        [TestMethod]
        public async Task AddInvalidProfile(string username, string firstName, string lastName)
        {
            try
            {
                var profile = new UserProfile(username, firstName, lastName);
                await store.AddProfile(profile);

                Assert.Fail($"Expected {nameof(ArgumentException)} was not thrown");
            }
            catch (ArgumentException)
            {
            }
        }

        [TestMethod]
        public async Task UpdateProfile()
        {
            await store.AddProfile(new UserProfile(testProfile.Username, "Foo", "Bar"));
            await store.UpdateProfile(testProfile);

            Assert.AreEqual(testProfile, await store.GetProfile(testProfile.Username));
        }

        private static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.test.json")
            .Build();
            return config;
        }

    }
}