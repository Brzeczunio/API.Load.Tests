using NBomber.Http.CSharp;
using System.Net.Http.Headers;
using System.Text;
using HttpMethod = API.Load.Tests.Enums.HttpMethod;

namespace API.Load.Tests.Builders
{
    internal static class RequestBuilder
    {
        public static void Authorize(this HttpRequestMessage httpRequestMessage, string token)
        {
            httpRequestMessage.WithHeader("Authorization", new StringBuilder().Append("bearer ").Append(token).ToString());
        }

        public static HttpRequestMessage CreateGetRequest(string url) => Http.CreateRequest(HttpMethod.GET.ToString(), url);

        public static HttpRequestMessage CreatePostRequest<T>(string url, T jsonBody)
        {
            var request = Http.CreateRequest(HttpMethod.POST.ToString(), url)
                .WithJsonBody(jsonBody);
            request.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        public static HttpRequestMessage CreatePutRequest<T>(string url, T jsonBody)
        {
            var request = Http.CreateRequest(HttpMethod.PUT.ToString(), url)
                .WithJsonBody(jsonBody);
            request.Content!.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            return request;
        }

        public static HttpRequestMessage CreateDeleteRequest(string url) => Http.CreateRequest(HttpMethod.DELETE.ToString(), url);
    }
}
