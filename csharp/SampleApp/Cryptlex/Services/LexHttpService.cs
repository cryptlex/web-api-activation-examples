using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Mime;

namespace Cryptlex.Services
{

    public class LexHttpService
    {
        private const string _apiUrl = "https://api.cryptlex.com/v3/activations";

        public LexHttpService()
        {
        }

        public HttpResponseMessage CreateActivation(string postData)
        {
            return PostAsync($"{_apiUrl}", postData).Result;
        }

        public HttpResponseMessage UpdateActivation(string activationId, string postData)
        {
            return PatchAsync($"{_apiUrl}/{activationId}", postData).Result;
        }

        public HttpResponseMessage DeleteActivation(string activationId)
        {
            return DeleteAsync($"{_apiUrl}/{activationId}", null).Result;
        }

        private async Task<HttpResponseMessage> PostAsync(string url, string data)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };
            request.Content = new StringContent(data, Encoding.UTF8, LexConstants.JsonContentType);
            return await client.SendAsync(request);
        }

        private async Task<HttpResponseMessage> PatchAsync(string url, string data)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(LexConstants.HttpMethodPatch),
                RequestUri = new Uri(url)
            };
            request.Content = new StringContent(data, Encoding.UTF8, LexConstants.JsonContentType);
            return await client.SendAsync(request);
            // var json = await response.Content.ReadAsStringAsync();
        }

        private async Task<HttpResponseMessage> DeleteAsync(string url, string data)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            return await client.SendAsync(request);
        }
    }
}