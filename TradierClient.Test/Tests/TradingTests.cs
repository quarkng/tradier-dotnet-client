using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using NUnit.Framework;
using Tradier.Client;
using TradierClient.Test.Helpers;
using Tradier.Client.Models.Trading;
using System.Drawing;
using System.Diagnostics;
using Tradier.Client.Models.MarketData;

namespace TradierClient.Test.Tests
{
    internal class TradingTests
    {
        private readonly bool _isSandbox = true;
        //private readonly bool _isSandbox = false;

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
            if (_isSandbox)
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

            _client.Trading.KeepJson = true;
        }

        [Test]
        [TestCase("SPY", "SPY250117C00520000", "buy_to_open", 1, "market", "day", null, null, true)]
        public async Task PlaceOptionOrder(string symbol, string optionSymbol, string side, int quantity, string type, string duration, double? price = null, double? stop = null, bool preview = false)
        {
            preview |= !_isSandbox; // Only allow preview if using Production endpoint

            var result = await _client.Trading.PlaceOptionOrder(symbol, optionSymbol, side, quantity, type, duration, price, stop, preview);
            Assert.IsNotNull(result);

            if (preview)
            {
                Assert.IsTrue((result as OrderPreviewResponse).Symbol == symbol);
            }
            else
            {
                Assert.IsTrue((result as OrderResponse).Id > 0);
            }
        }

        [Test]
        [TestCase("SPY", "market", "day", "SPY250117C00530000,buy_to_open,1;SPY250117C00560000,sell_to_open,1", null, true)]
        public async Task PlaceMultilegOrder(string symbol, string type, string duration, string legs, double? price = null, bool preview = false)
        {
            preview |= !_isSandbox; // Only allow preview if using Production endpoint
            var legsConverted = StringToLegs(legs);

            var result = await _client.Trading.PlaceMultilegOrder(symbol, type, duration, legsConverted, price, preview);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Status == "ok");
            if (preview) Assert.IsTrue(((OrderPreviewResponse)result).ClassOrder == "multileg");
        }

        private List<(string, string, int)> StringToLegs(string str)
        {
            var result = new List<(string, string, int)>();
            var stringLegs = str.Split(';');

            foreach (var s in stringLegs)
            {
                result.Add(StringToLeg(s));
            }

            return result;
        }

        private (string, string, int) StringToLeg(string str)
        {
            var parts = str.Split(',');
            return (parts[0], parts[1], int.Parse(parts[2]));
        }

        [Test]
        [TestCase("SPY","buy",10,"market","day",null,null,true)]
        public async Task PlaceEquityOrder(string symbol, string side, int quantity, string type, string duration, double? price = null, double? stop = null, bool preview = false)
        {
            preview |= !_isSandbox; // Only allow preview if using Production endpoint

            var result = await _client.Trading.PlaceEquityOrder(symbol, side, quantity, type, duration, price, stop, preview);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Status == "ok");
        }

        [Test]
        [TestCase("SPY", "market", "day", ",buy,100;SPY250117C00535000,sell_to_open,1;SPY250117C00545000,buy_to_open,1", null, true)]
        public async Task PlaceComboOrder(string symbol, string type, string duration, string legsStr, double? price = null, bool preview = false)
        {
            preview |= !_isSandbox; // Only allow preview if using Production endpoint

            var legs = StringToLegs(legsStr);
            var result = await _client.Trading.PlaceComboOrder(symbol, type, duration, legs, price, preview);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Status == "ok");
        }

        [Test]
        [TestCase("day", "SPY,1,limit,SPY250117C00535000,buy_to_open,1.5,null;SPY,1,market,SPY250117C00545000,sell_to_open,null,null", true)]
        public async Task PlaceOtoOrder(string duration, string legsStr, bool preview = false)
        {
            preview |= !_isSandbox; // Only allow preview if using Production endpoint

            var legs = StringToOtoOcoLegs(legsStr);
            var result = await _client.Trading.PlaceOtoOrder(duration, legs, preview);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Status == "ok");
            Assert.AreEqual("oto", ((OrderPreviewResponse)result).ClassOrder);
        }

        //............Symbol, qty, type, option-sym, side, price, stop
        private List<(string, int, string, string, string, double?, double?)> StringToOtoOcoLegs(string str) 
        {
            var result = new List<(string, int, string, string, string, double?, double?)>();
            var stringLegs = str.Split(';');

            foreach (var s in stringLegs)
            {
                result.Add(StringToOtoOcoLeg(s));
            }

            return result;
        }

        private (string, int, string, string, string, double?, double?) StringToOtoOcoLeg(string str)
        {
            var parts = str.Split(',');

            double d;
            double? price = double.TryParse(parts[5], out d) ? d : null;
            double? stop= double.TryParse(parts[6], out d) ? d : null;

            return (parts[0], int.Parse(parts[1]), parts[2], parts[3], parts[4], price, stop );
        }

        [Test]
        [TestCase("day", "SPY,1,limit,SPY250117C00535000,buy_to_open,29,null;SPY,1,stop_limit,SPY250117C00535000,buy_to_open,33.5,33", true)]
        public async Task PlaceOcoOrder(string duration, string legsStr, bool preview = false)
        {
            preview |= !_isSandbox; // Only allow preview if using Production endpoint

            var legs = StringToOtoOcoLegs(legsStr);
            var result = await _client.Trading.PlaceOcoOrder(duration, legs, preview);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Status == "ok");
            Assert.AreEqual("oco", ((OrderPreviewResponse)result).ClassOrder);
        }

        [Test]
        [TestCase("day", "SPY,1,limit,SPY250117C00535000,buy_to_open,9.16,null;SPY,1,limit,SPY250117C00535000,sell_to_close,14.5,null;SPY,1,stop_limit,SPY250117C00535000,sell_to_close,13.9,14.0", true)]
        public async Task PlaceOtocoOrder(string duration, string legsStr, bool preview = false)
        {
            preview |= !_isSandbox; // Only allow preview if using Production endpoint

            var legs = StringToOtoOcoLegs(legsStr);
            var result = await _client.Trading.PlaceOtocoOrder(duration, legs, preview);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Status == "ok");
            Assert.AreEqual("otoco", ((OrderPreviewResponse)result).ClassOrder);
        }


        //        public async Task<OrderResponse> ModifyOrder(int orderId, string type = null, string duration = null, double? price = null, double? stop = null)


        //        public async Task<OrderResponse> CancelOrder(int orderId)




    }
}
