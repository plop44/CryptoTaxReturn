using System.Net.Http;
using System.Threading.Tasks;

namespace Common.Rest
{
    public static class ApiClientExtensions
    {
        public static Task<string> CallAsync(this BinanceRestClient restClient, HttpMethod method, string endpoint)
        {
            return restClient.CallAsync(method, endpoint, null);
        }
    }
}