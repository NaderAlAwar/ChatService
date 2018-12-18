using System;
using System.Threading.Tasks;
using ChatService.Storage;
using ChatService.Storage.Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using Moq;

namespace ChatServiceTests
{
    [TestClass]
    [TestCategory("UnitTests")]
    public class AzureTableProfileStoreTests
    {
        private Mock<ICloudTable> tableMock;
        private AzureTableProfileStore store;

        private readonly UserProfile testProfile = new UserProfile(Guid.NewGuid().ToString(), "Nehme", "Bilal");

        [TestInitialize]
        public void TestInitialize()
        {
            tableMock = new Mock<ICloudTable>();
            store = new AzureTableProfileStore(tableMock.Object);

            tableMock.Setup(m => m.ExecuteAsync(It.IsAny<TableOperation>()))
                .ThrowsAsync(new StorageException(new RequestResult { HttpStatusCode = 503 }, "Storage is down", null));
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

        [DataRow("")]
        [DataRow(null)]
        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task DeleteInvalidProfile(string username)
        {
            await store.TryDelete(username);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task DeleteProfile_StorageIsUnavailable()
        {
            await store.TryDelete("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task GetProfile_StorageIsUnavailable()
        {
            await store.GetProfile("foo");
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task AddProfile_StorageIsUnavailable()
        {
            await store.AddProfile(testProfile);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task UpdateProfile_StorageIsUnavailable()
        {
            await store.UpdateProfile(testProfile);
        }

        [TestMethod]
        [ExpectedException(typeof(StorageErrorException))]
        public async Task TryDelete_StorageIsUnavailable()
        {
            await store.TryDelete("foo");
        }
    }
}