using System;
using System.Collections.Generic;
using System.Text;

using Newtonsoft.Json;
using System.Collections.Generic;

namespace Nop.Plugin.Payments.MastercardGateway.Models
{

    public class MastercardSessionResponse
    {
        public string Result { get; set; }
        public SessionData Session { get; set; }

        public class SessionData
        {
            public string Id { get; set; }
            public string Version { get; set; }
        }
    }

    public class MastercardOrderResponse
    {
        public string Result { get; set; }
        public string ResponseCode { get; set; }
        public string OrderId { get; set; }
    }

    public class ResponseModel
        {
            [JsonProperty("success")]
            public bool Success { get; set; }

            [JsonProperty("orderId")]
            public int OrderId { get; set; }

            [JsonProperty("transactionId")]
            public string TransactionId { get; set; }

            [JsonProperty("message")]
            public string Message { get; set; }

            [JsonProperty("response")]
            public PaymentResponse Response { get; set; }
        }

        public class PaymentResponse
        {
            [JsonProperty("result")]
            public string result { get; set; }

            [JsonProperty("response")]
            public GatewayResponse GatewayResponse { get; set; }

            [JsonProperty("session")]
            public SessionResponse Session { get; set; }

            [JsonProperty("order")]
            public OrderResponse Order { get; set; }

            [JsonProperty("sourceOfFunds")]
            public SourceOfFunds SourceOfFunds { get; set; }

            [JsonProperty("transaction")]
            public List<Transaction> Transactions { get; set; }

            [JsonProperty("timeOfRecord")]
            public string TimeOfRecord { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }
        }

        public class GatewayResponse
        {
            [JsonProperty("gatewayCode")]
            public string GatewayCode { get; set; }
        }

        public class SessionResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("version")]
            public string Version { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }
        }

        public class OrderResponse
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("totalAuthorizedAmount")]
            public decimal TotalAuthorizedAmount { get; set; }

            [JsonProperty("totalCapturedAmount")]
            public decimal TotalCapturedAmount { get; set; }

            [JsonProperty("totalRefundedAmount")]
            public decimal TotalRefundedAmount { get; set; }
        }

        public class SourceOfFunds
        {
            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("provided")]
            public ProvidedCard Card { get; set; }
        }

        public class ProvidedCard
        {
            [JsonProperty("brand")]
            public string Brand { get; set; }

            [JsonProperty("number")]
            public string MaskedNumber { get; set; }

            [JsonProperty("expiry")]
            public ExpiryDate Expiry { get; set; }
        }

        public class ExpiryDate
        {
            [JsonProperty("month")]
            public string Month { get; set; }

            [JsonProperty("year")]
            public string Year { get; set; }
        }

        public class Transaction
        {
            [JsonProperty("id")]
            public string Id { get; set; }

            [JsonProperty("type")]
            public string Type { get; set; }

            [JsonProperty("amount")]
            public decimal Amount { get; set; }

            [JsonProperty("currency")]
            public string Currency { get; set; }

            [JsonProperty("status")]
            public string Status { get; set; }

            [JsonProperty("timeOfRecord")]
            public string TimeOfRecord { get; set; }

            [JsonProperty("authorizationCode")]
            public string AuthorizationCode { get; set; }

            [JsonProperty("source")]
            public string Source { get; set; }
        }

        public class CheckoutSessionResponse
        {
            [JsonProperty("session")]
            public SessionResponse Session { get; set; }

            [JsonProperty("successIndicator")]
            public string SuccessIndicator { get; set; }
        }
    public class SessionData
    {
        public string result { get; set; }
        public Session session { get; set; }

        public class Session
        {
            public string id { get; set; }
            public string version { get; set; }
        }
    }
}



