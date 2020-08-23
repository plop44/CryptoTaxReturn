using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Common.Rest;
using Newtonsoft.Json;

namespace Extract.KuCoin
{
    public class KuCoinRestClient
    {
        private readonly KuCoinKeyAndSecret _kuCoinKeyAndSecret;
        private readonly HttpClient _httpClient;

        public KuCoinRestClient(KuCoinKeyAndSecret kuCoinKeyAndSecret)
        {
            _kuCoinKeyAndSecret = kuCoinKeyAndSecret;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(@"https://api.kucoin.com")
            };
            
            _httpClient.DefaultRequestHeaders
                .Add("KC-API-KEY", kuCoinKeyAndSecret.Key);

            _httpClient.DefaultRequestHeaders
                .Add("KC-API-PASSPHRASE", kuCoinKeyAndSecret.PassPhrase);

            _httpClient.DefaultRequestHeaders
                .Add("User-Agent", "Mozilla/4.0 (compatible; MSIE 7.0; Windows NT 5.1)");

            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> CallAsync(HttpMethod method, string endpoint, string parameters)
        {
            var finalEndpoint = endpoint + (parameters != null ? "?" + parameters : null);
            var request = new HttpRequestMessage(method, finalEndpoint);

            var timestamp = GenerateTimeStamp();
            request.Headers.Add("KC-API-TIMESTAMP", timestamp);
            request.Headers.Add("KC-API-SIGN", GetSignature(method, finalEndpoint, timestamp, null));

            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return body;

            if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                throw new Exception("Api Request Timeout.");

            throw new Exception($"{finalEndpoint}\n{body}");
        }
        private string GetSignature(HttpMethod method, string endpoint, string timestamp, Dictionary<string, object> body)
        {
            var callMethod = method.ToString().ToUpper();

            var boydSerialized = body != null && body.Count > 0
                ? JsonConvert.SerializeObject(body)
                : string.Empty;

            var sigString = $"{timestamp}{callMethod}{endpoint}{boydSerialized}";

            var signature = HmacSha256(sigString);

            return signature;
        }
        private string GenerateTimeStamp()
        {
            var dtOffset = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            return dtOffset.ToUnixTimeMilliseconds().ToString();
        }

        private string HmacSha256(string message)
        {
            var encoding = new ASCIIEncoding();
            var msgBytes = encoding.GetBytes(message);
            var secretBytes = encoding.GetBytes(_kuCoinKeyAndSecret.Secret);
            using (var hmac = new HMACSHA256(secretBytes))
            {
                var hash = hmac.ComputeHash(msgBytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}