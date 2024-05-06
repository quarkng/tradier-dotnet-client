using System;
using System.Configuration;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Tradier.Client;
using Tradier.Client.Models.MarketData;
using TradierClient.Test.Helpers;

namespace TradierClient.Test.Tests
{
    public class MarketDataTests
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
        }

        [Test]
        [TestCase("SPY")]
        public async Task GetTimeSales_5min(string symbol)
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");

            string interval = "5min";
            DateTime start = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow.AddDays(-40), easternZone);
            DateTime end = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);
            string filter = "open"; // 40 days of data available when filter is "open"

            var result = await _client.MarketData.GetTimeSales(symbol, interval, start, end, filter);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Data.Count > 0);

            var first = result.Data.First();
            var last = result.Data.Last();

            Assert.IsNotNull(first);
            Assert.IsNotNull(last);
        }

        [Test]
        [TestCase("MSFT", false)]
        public async Task PostGetQuotesForSingleSymbol(string symbols, bool greeks)
        {
            var result = await _client.MarketData.PostGetQuotes(symbols, greeks);
            Assert.IsNotNull(result.Quote.First());
            Assert.AreEqual(1, result.Quote.Count);
        }

        [Test]
        [TestCase("GME", "daily")]
        public async Task GetMultiDayHistoricalQuotesTest(string symbol, string interval)
        {
            var start = TimingHelper.GetLastWednesday();
            var end = TimingHelper.GetLastThursday();
            var result = await _client.MarketData.GetHistoricalQuotes(symbol, interval, start, end);
            Assert.IsNotNull(result.Day);
            Assert.AreEqual(2, result.Day.Count);

            var firstDay = result.Day.First();
            var secondDay = result.Day.Last();
            Assert.AreEqual(start.ToString("yyyy-MM-dd"), firstDay.Date);
            Assert.NotZero(firstDay.Open);
            Assert.AreEqual(end.ToString("yyyy-MM-dd"), secondDay.Date);
            Assert.NotZero(secondDay.Open);
        }

        [Test]
        [TestCase("SPY", 300)]
        public async Task GetManyDayHistoricalQuotes(string symbol, int months)
        {
            TimeZoneInfo easternZone = TimeZoneInfo.FindSystemTimeZoneById("Eastern Standard Time");
            DateTime nowEastern = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, easternZone);

            string interval = "daily";
            DateTime start = nowEastern.AddMonths(-1 * months);
            DateTime end = nowEastern;

            var result = await _client.MarketData.GetHistoricalQuotes(symbol, interval, start, end);
            Assert.IsNotNull(result.Day);

            var firstDay = result.Day.First();
            var lastDay = result.Day.Last();
            Assert.AreEqual(start.ToString("yyyy-MM-dd"), firstDay.Date);
            Assert.NotZero(firstDay.Open);
            Assert.AreEqual(end.ToString("yyyy-MM-dd"), lastDay.Date);
            Assert.NotZero(lastDay.Open);
        }

        [Test]
        [TestCase("SPY", "1993-01-25")]
        public async Task GetManyWeeksHistoricalQuotes(string symbol, string fromDate)
        {
            string interval = "weekly";
            DateTime start = DateTime.Parse(fromDate);
            DateTime end = DateTime.Now;

            var result = await _client.MarketData.GetHistoricalQuotes(symbol, interval, start, end);
            Assert.IsNotNull(result.Day);

            var firstDay = result.Day.First();
            var lastDay = result.Day.Last();
            Assert.AreEqual(start.ToString("yyyy-MM-dd"), firstDay.Date);
            Assert.NotZero(firstDay.Open);
            Assert.AreEqual(end.ToString("yyyy-MM-dd"), lastDay.Date);
            Assert.NotZero(lastDay.Open);
        }

        [Test]
        [TestCase("GME", "daily")]
        public async Task GetSingleDayHistoricalQuotesTest(string symbol, string interval)
        {
            var start = TimingHelper.GetLastWednesday();
            var end = TimingHelper.GetLastWednesday();
            var result = await _client.MarketData.GetHistoricalQuotes(symbol, interval, start, end);
            Assert.IsNotNull(result.Day);
            Assert.AreEqual(1, result.Day.Count);

            var firstDay = result.Day.First();
            Assert.AreEqual(start.ToString("yyyy-MM-dd"), firstDay.Date);
            Assert.NotZero(firstDay.Open);
        }

        [Test]
        [TestCase("AAPL")]
        [TestCase("AAPL,GOOG")]
        public async Task GetCompanyInfoTest(string symbols)  // Not availible in Paper Trade (Sandbox)
        {
            var result = await _client.MarketData.GetCompany(symbols);

            var companyData = result.FirstOrDefault().Results.FirstOrDefault(x => x.Tables?.CompanyProfile != null);
            Assert.IsNotNull(companyData);
            Assert.IsNotNull(companyData?.Tables?.CompanyProfile?.CompanyId);
        }

        [Test]
        [TestCase("AAPL")]
        [TestCase("AAPL,GOOG")]
        public async Task GetCorporateCalendarTest(string symbols)  // Not availible in Paper Trade (Sandbox)
        {
            var result = await _client.MarketData.GetCorporateCalendars(symbols);

            var companyData = result.FirstOrDefault().Results.FirstOrDefault(x => x.Tables?.CorporateCalendars != null);
            Assert.IsNotNull(companyData);
            Assert.IsNotNull(companyData?.Tables?.CorporateCalendars?.FirstOrDefault()?.CompanyId);
        }

        [Test]
        public async Task GetEtbSecuritiesTest()
        {
            var result = await _client.MarketData.GetEtbSecurities();
            Assert.IsNotNull(result.Security.FirstOrDefault()?.Symbol);
        }

        [Test]
        [TestCase("Alphabet")]
        public async Task SearchCompaniesTest(string symbol)
        {
            var result = await _client.MarketData.SearchCompanies(symbol);
            Assert.IsNotNull(result.Security.FirstOrDefault()?.Symbol);
        }

        [Test]
        [TestCase("GOOG")]
        public async Task LookupSymbolTest(string query)
        {
            var result = await _client.MarketData.LookupSymbol(query);
            Assert.IsNotNull(result.Security.FirstOrDefault()?.Symbol);
        }
    }
}