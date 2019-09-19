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
            return Post($"{_apiUrl}", postData);
        }

        public HttpResponseMessage UpdateActivation(string activationId, string postData)
        {
            return Patch($"{_apiUrl}/{activationId}", postData);
        }

        public HttpResponseMessage DeleteActivation(string activationId)
        {
            return Delete($"{_apiUrl}/{activationId}", null);
        }

        private HttpResponseMessage Post(string url, string data)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(url)
            };
            request.Content = new StringContent(data, Encoding.UTF8, LexConstants.JsonContentType);
            return client.SendAsync(request).Result;
        }

        private HttpResponseMessage Patch(string url, string data)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = new HttpMethod(LexConstants.HttpMethodPatch),
                RequestUri = new Uri(url)
            };
            request.Content = new StringContent(data, Encoding.UTF8, LexConstants.JsonContentType);
            return client.SendAsync(request).Result;
        }

        private HttpResponseMessage Delete(string url, string data)
        {
            var client = new HttpClient();
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Delete,
                RequestUri = new Uri(url)
            };
            return client.SendAsync(request).Result;
        }
    }
}