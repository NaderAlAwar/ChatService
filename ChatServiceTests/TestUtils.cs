using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceTests
{
    public static class TestUtils
    {
        /// <summary>
        /// Used to avoid duplicating this same code in several tests
        /// </summary>
        public static void AssertStatusCode(HttpStatusCode statusCode, IActionResult actionResult)
        {
            Assert.IsTrue(actionResult is ObjectResult);
            ObjectResult objectResult = (ObjectResult)actionResult;

            Assert.AreEqual((int)statusCode, objectResult.StatusCode);
        }
    }
}
