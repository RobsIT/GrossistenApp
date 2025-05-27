using System.Text.Json;

namespace GrossistenApp.Service
{
    public class CallApiService
    {
        private readonly IHttpClientFactory _httpClientFactory;

        public CallApiService(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory;
        }

        public async Task<T> GetDataFromApi<T>(string url)
        {
            var client = _httpClientFactory.CreateClient("MyCallApiService");
            var response = await client.GetStringAsync(url);

            var data = JsonSerializer.Deserialize<T>(response, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            return data;
        }

        public async Task<HttpResponseMessage> CreateItem<T>(string url, T newItem)
        {
            var client = _httpClientFactory.CreateClient("MyCallApiService");
            var response = await client.PostAsJsonAsync(url, newItem);
            return response;
        }

        public async Task<HttpResponseMessage> EditItem<T>(string url, T updatedItem)
        {
            var client = _httpClientFactory.CreateClient("MyCallApiService");
            var response = await client.PutAsJsonAsync(url, updatedItem);
            return response;
        }

        public async Task<HttpResponseMessage> DeleteItem(string url)
        {
            var client = _httpClientFactory.CreateClient("MyCallApiService");
            var response = await client.DeleteAsync(url);
            return response;
        }
    }
}
