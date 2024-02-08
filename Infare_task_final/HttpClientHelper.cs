using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Infare_task_final
{
    // Class utilize HttpClient
    public static class HttpClientHelper
    {
        private static readonly HttpClient client = new HttpClient();

        public static async Task<(string responseBody, string contentType, HttpStatusCode statusCode)> GetAsync(string url)
        {
            HttpResponseMessage response = await client.GetAsync(url);
            string responseBody = await response.Content.ReadAsStringAsync();
            // Ensure a null-safe way to fetch the content type
            string contentType = response.Content.Headers.ContentType?.MediaType ?? "unknown";
            HttpStatusCode statusCode = response.StatusCode;

            return (responseBody, contentType, statusCode);
        }
    }
}
