using Newtonsoft.Json;
using System;

namespace Tradier.Client.Models.Trading
{
    public class OrderResponseRootobject
    {
        [JsonProperty("order")]
        public OrderResponse OrderReponse { get; set; }
    }

    public class OrderResponse : IOrder
    {
        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("status")]
        public string Status { get; set; }

        [JsonProperty("partner_id")]
        public string PartnerId { get; set; }
    }

    public class OrderReponseWithJson : OrderResponse
    {
        public string Json { get; set; }

        internal OrderReponseWithJson( OrderResponse orderResp, string json ) 
        { 
            Id = orderResp.Id;
            Status= orderResp.Status;
            PartnerId= orderResp.PartnerId;
            Json = json;
        }
    }

}
