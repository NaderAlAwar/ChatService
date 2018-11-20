using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ChatServiceTests.Utils
{
    public static class TestUtils
    {
        private static readonly Random random = new Random();

        /// <summary>
        /// Used to avoid duplicating this same code in several tests
        /// </summary>
        public static void AssertStatusCode(HttpStatusCode statusCode, IActionResult actionResult)
        {
            Assert.IsTrue(actionResult is ObjectResult);
            ObjectResult objectResult = (ObjectResult)actionResult;

            Assert.AreEqual((int)statusCode, objectResult.StatusCode);
        }

        public static IConfiguration InitConfiguration()
        {
            var config = new ConfigurationBuilder()
            .AddJsonFile("appsettings.test.json")
            .Build();
            return config;
        }
    }
}
