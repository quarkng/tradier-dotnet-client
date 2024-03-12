using Newtonsoft.Json;
using System.Linq;
using System.Threading.Tasks;
using Tradier.Client.Exceptions;
using Tradier.Client.Helpers;
using Tradier.Client.Models.Account;

// ReSharper disable once CheckNamespace
namespace Tradier.Client
{
    /// <summary>
    /// The <c>Account</c> class. 
    /// </summary>
    public class Account
    {
        private readonly Requests _requests;
        private readonly string _defaultAccountNumber;

        public bool KeepJson { get; set; }

        /// <summary>
        /// Account Constructor
        /// </summary>
        public Account(Requests requests, string defaultAccountNumber)
        {
            _requests = requests;
            _defaultAccountNumber = defaultAccountNumber;
        }

        /// <summary>
        /// The user’s profile contains information pertaining to the user and his/her accounts
        /// </summary>
        public async Task<Profile> GetUserProfile()
        {
            var response = await _requests.GetRequest("user/profile");
            var result = JsonConvert.DeserializeObject<ProfileRootObject>(response).Profile;

            if (KeepJson && result != null)
            {
                result.Json = response;
            }
            return result;
        }

        /// <summary>
        /// Get balances information for a specific or a default user account.
        /// </summary>
        public async Task<Balances> GetBalances(string accountNumber = null)
        {
            accountNumber = string.IsNullOrEmpty(accountNumber) ? _defaultAccountNumber : accountNumber;

            if (string.IsNullOrEmpty(accountNumber))
            {
                throw new MissingAccountNumberException();
            }

            var response = await _requests.GetRequest($"accounts/{accountNumber}/balances");
            var result = JsonConvert.DeserializeObject<BalanceRootObject>(response).Balances;

            if (KeepJson && result != null)
            {
                result.Json = response;
            }

            return result;
        }

        /// <summary>
        /// Get the current positions being held in an account. These positions are updated intraday via trading
        /// </summary>
        public async Task<Positions> GetPositions(string accountNumber = null)
        {
            accountNumber = string.IsNullOrEmpty(accountNumber) ? _defaultAccountNumber : accountNumber;

            if (string.IsNullOrEmpty(accountNumber))
            {
                throw new MissingAccountNumberException();
            }

            var response = await _requests.GetRequest($"accounts/{accountNumber}/positions");
            var result = JsonConvert.DeserializeObject<PositionsRootobject>(response).Positions;

            if (KeepJson && result != null)
            {
                result.Json = response;
            }
            return result;
        }

        /// <summary>
        /// Get historical activity for the default account
        /// </summary>
        public async Task<History> GetHistory(int page = 1, int limitPerPage = 25)
        {
            if (string.IsNullOrEmpty(_defaultAccountNumber))
            {
                throw new MissingAccountNumberException("The default account number was not defined.");
            }

            return await GetHistory(_defaultAccountNumber, page, limitPerPage);
        }

        /// <summary>
        /// Get historical activity for an account
        /// </summary>
        public async Task<History> GetHistory(string accountNumber, int page = 1, int limitPerPage = 25)
        {
            var response = await _requests.GetRequest($"accounts/{accountNumber}/history?page={page}&limit={limitPerPage}");
            var result = JsonConvert.DeserializeObject<HistoryRootobject>(response).History;

            if (KeepJson && result != null)
            {
                result.Json = response;
            }
            return result;
        }

        /// <summary>
        /// Get cost basis information for the default user account
        /// </summary>
        public async Task<GainLoss> GetGainLoss(int page = 1, int limitPerPage = 25)
        {
            if (string.IsNullOrEmpty(_defaultAccountNumber))
            {
                throw new MissingAccountNumberException("The default account number was not defined.");
            }

            return await GetGainLoss(_defaultAccountNumber, page, limitPerPage);
        }

        /// <summary>
        /// Get cost basis information for a specific user account
        /// </summary>
        public async Task<GainLoss> GetGainLoss(string accountNumber, int page = 1, int limitPerPage = 25)
        {
            var response = await _requests.GetRequest($"accounts/{accountNumber}/gainloss?page={page}&limit={limitPerPage}");
            var result = JsonConvert.DeserializeObject<GainLossRootobject>(response).GainLoss;

            if (KeepJson && result != null)
            {
                result.Json = response; 
            }
            return result;
        }

        /// <summary>
        /// Retrieve orders placed within an account
        /// </summary>
        public async Task<Orders> GetOrders(string accountNumber = null)
        {
            accountNumber = string.IsNullOrEmpty(accountNumber) ? _defaultAccountNumber : accountNumber;

            if (string.IsNullOrEmpty(accountNumber))
            {
                throw new MissingAccountNumberException();
            }

            if (string.IsNullOrEmpty(accountNumber))
            {
                throw new MissingAccountNumberException();
            }

            var response = await _requests.GetRequest($"accounts/{accountNumber}/orders");
            var resultRoot = JsonConvert.DeserializeObject<OrdersRootobject>(response);

            var result = resultRoot?.Orders ?? new OrdersWithJson { Json = response };

            if (KeepJson)
            {
                result.Json = response;
            }
            return result;
        }

        /// <summary>
        /// Get detailed information about a previously placed order in the default account
        /// </summary>
        public async Task<Order> GetOrder(int orderId)
        {
            if (string.IsNullOrEmpty(_defaultAccountNumber))
            {
                throw new MissingAccountNumberException("The default account number was not defined.");
            }

            return await GetOrder(_defaultAccountNumber, orderId);
        }

        /// <summary>
        /// Get detailed information about a previously placed order
        /// </summary>
        public async Task<Order> GetOrder(string accountNumber, int orderId)
        {
            var response = await _requests.GetRequest($"accounts/{accountNumber}/orders/{orderId}");
            var result =  JsonConvert.DeserializeObject<Orders>(response).Order.FirstOrDefault();
            return result;
        }
    }
}