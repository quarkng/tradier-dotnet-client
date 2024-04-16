using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using NUnit.Framework;
using Tradier.Client;
using TradierClient.Test.Helpers;

namespace TradierClient.Test.Tests
{
    internal class AccountTests
    {
        //private readonly bool _isSandbox = true;
        private readonly bool _isSandbox = false;

        private Tradier.Client.TradierClient _client;
        private Settings _settings;

        [SetUp]
        public void Init()
        {
            _settings = Configuration.GetApplicationConfiguration(TestContext.CurrentContext.TestDirectory);
        }

        [SetUp]
        public void Setup()
        {
            if( _isSandbox )
            {   // Use SandBox API Token and endpoint
                var sandboxApiToken = _settings.SandboxApiToken;
                var sandboxAccountNumber = _settings.SandboxAccountNumber;
                _client = new Tradier.Client.TradierClient(sandboxApiToken, sandboxAccountNumber);
            }
            else
            {   //Use Production API Token and endpoint
                var apiToken = _settings.ApiToken;
                var accountNumber = _settings.AccountNumber;
                _client = new Tradier.Client.TradierClient(apiToken, accountNumber, true);
            }

            _client.Account.KeepJson = true;
        }

        [Test]
        public async Task GetUserProfile()
        {
            var result = await _client.Account.GetUserProfile();
            Assert.IsNotNull(result.Account.FirstOrDefault());
            Assert.IsNotNull(result.Name);
        }

        [Test]
        public async Task GetBalances()
        {
            var result = await _client.Account.GetBalances();
            Assert.IsTrue(result.TotalEquity > 0);
            Assert.IsNotNull(result.AccountNumber);
            Assert.IsNotEmpty(result.AccountNumber);
        }

        [Test]
        public async Task GetPositions()
        {
            var result = await _client.Account.GetPositions();
            Assert.IsNotNull(result.Position);
        }

        [Test]
        public async Task GetHistory() // Not available in Paper Trading (sandbox)
        {
            var result = await _client.Account.GetHistory();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Event);
        }

        [Test]
        public async Task GetGainLoss() // Not available in Paper Trading (sandbox)
        {
            var result = await _client.Account.GetGainLoss();
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.ClosedPosition);
        }

        [Test]
        public async Task GetOrders()
        {
            var result = await _client.Account.GetOrders();
            Assert.IsNotNull(result);
        }

        [Test]
        [TestCase(10768446)]
        //[TestCase(0)]
        public async Task GetOrder(int orderId)
        {
            var result = await _client.Account.GetOrder(orderId);
            Assert.IsNotNull(result);
        }

    }
}
