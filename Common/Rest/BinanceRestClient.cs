using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Common.Rest
{
    public sealed class BinanceRestClient
    {
        private readonly BinanceKeyAndSecret _binanceKeyAndSecret;
        private readonly HttpClient _httpClient;

        public BinanceRestClient(BinanceKeyAndSecret binanceKeyAndSecret)
        {
            _binanceKeyAndSecret = binanceKeyAndSecret;
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri(@"https://api.binance.com")
            };

            _httpClient.DefaultRequestHeaders
                .Add("X-MBX-APIKEY", binanceKeyAndSecret.Key);

            _httpClient.DefaultRequestHeaders
                .Accept
                .Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<string> CallAsync(HttpMethod method, string endpoint, string parameters)
        {
            var finalEndpoint = endpoint + (parameters != null ? "?" + parameters : null);
            var request = new HttpRequestMessage(method, finalEndpoint);
            
            var response = await _httpClient.SendAsync(request).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (response.IsSuccessStatusCode) return body;

            if (response.StatusCode == HttpStatusCode.GatewayTimeout)
                throw new Exception("Api Request Timeout.");

            throw new Exception($"{finalEndpoint}\n{body}");
        }

        public string GenerateTimeStamp()
        {
            var dtOffset = new DateTimeOffset(DateTime.Now.ToUniversalTime());
            return dtOffset.ToUnixTimeMilliseconds().ToString();
        }
        private string AddSignature(string parameters)
        {
            var signature = GenerateSignature(parameters);
            parameters += "&signature=" + signature;
            return parameters;
        }
        private string GenerateSignature(string parameters)
        {
            var key = Encoding.UTF8.GetBytes(_binanceKeyAndSecret.Secret);
            string stringHash;
            using (var hmac = new HMACSHA256(key))
            {
                var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(parameters));
                stringHash = BitConverter.ToString(hash).Replace("-", "");
            }

            return stringHash;
        }

        public string AddTimestampAndSign(string parameter)
        {
            var addTimestampAndSign = string.IsNullOrEmpty(parameter) ? $"timestamp={GenerateTimeStamp()}" : $"{parameter}&timestamp={GenerateTimeStamp()}";
            return AddSignature(addTimestampAndSign);
        }
    }
}