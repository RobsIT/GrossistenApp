using System.Net;
using System.Text;
using System.Text.Json;
using GrossistenApp.Models;
using GrossistenApp.Service;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Moq.Protected;
using Xunit;

namespace GrossistenTest
{
    public class CallApiServiceTests
    {
        private readonly Mock<IHttpClientFactory> _mockHttpClientFactory;
        private readonly Mock<HttpMessageHandler> _mockHttpMessageHandler;
        private readonly HttpClient _httpClient;
        private readonly CallApiService _callApiService;

        public CallApiServiceTests()
        {
            //creates mock objects that will act ass IHttpClientFactory and HttpMessageHandler
            _mockHttpClientFactory = new Mock<IHttpClientFactory>();
            _mockHttpMessageHandler = new Mock<HttpMessageHandler>();

            //creates a fake httpclient so we don't make real http calls
            _httpClient = new HttpClient(_mockHttpMessageHandler.Object);
            _mockHttpClientFactory.Setup(x => x.CreateClient("MyCallApiService"))
                                  .Returns(_httpClient);

            //creates the service that is tested
            _callApiService = new CallApiService(_mockHttpClientFactory.Object);
        }

        [Fact]
        public async Task GetDataFromApiValidResponseReturnsCorrectObject()
        {
            // test data is created
            var testUrl = "https://api.example.com/data";
            var expectedData = new Product { Id = 1, Title = "Test" };
            var jsonResponse = JsonSerializer.Serialize(expectedData);


            //sets up the test method to try the get method and only answere to get requests
            //only runs if the requestedUri is the same as TestUrl
            //and look at the HttpRequestMessages content
            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString() == testUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            // runs this method from callApiService to test it
            var result = await _callApiService.GetDataFromApi<Product>(testUrl);

            // expect it is not null, and if Id and title is correct
            Assert.NotNull(result);
            Assert.Equal(expectedData.Id, result.Id);
            Assert.Equal(expectedData.Title, result.Title);
        }

        [Fact]
        public async Task GetDataFromApiCaseInsensitivReturnsCorrectObject()
        {
            var testUrl = "https://api.example.com/data";
            var jsonResponse = """{"id": 1, "title": "Test"}""";
            var expectedData = new Product { Id = 1, Title = "Test" };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse, Encoding.UTF8, "application/json")
                });

            var result = await _callApiService.GetDataFromApi<Product>(testUrl);

            // expect it is not null, and if Id and title is correct
            Assert.NotNull(result);
            Assert.Equal(expectedData.Id, result.Id);
            Assert.Equal(expectedData.Title, result.Title);
        }

        [Fact]
        public async Task GetDataFromApiHttpClientThrowsExceptionWhenApiDown()
        {
            var testUrl = "https://api.example.com/data";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            //This test if we get the correct error when the api is down
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _callApiService.GetDataFromApi<Product>(testUrl));
        }

        [Fact]
        public async Task GetDataFromApiInvalidJsonThrowsJsonException()
        {
            var testUrl = "https://api.example.com/data";
            var invalidJson = "invalid json";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(invalidJson, Encoding.UTF8, "application/json")
                });

            //looks to see the correct message is thrown when invalid json happens
            await Assert.ThrowsAsync<JsonException>(() =>
                _callApiService.GetDataFromApi<Product>(testUrl));
        }

        [Fact]
        public async Task CreateItemValidItemReturnsSuccessResponse()
        {
            var testUrl = "https://api.example.com/product";
            var newItem = new Product { Id = 1, Title = "Test Title" };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Post &&
                        req.RequestUri.ToString() == testUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.Created,
                    Content = new StringContent("Created successfully")
                });

            var result = await _callApiService.CreateItem(testUrl, newItem);

            // Looks to see if the createItem ran correctly and you get the correct statusCode
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.Created, result.StatusCode);
        }

        [Fact]
        public async Task CreateItemHttpClientThrowsExceptionWhenApiDown()
        {
            var testUrl = "https://api.example.com/items";
            var newItem = new Product { Id = 1, Title = "New Item" };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Looks to see if you get the correct error message
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _callApiService.CreateItem(testUrl, newItem));
        }

        [Fact]
        public async Task CreateItemBadRequestReturnsErrorResponse()
        {
            var testUrl = "https://api.example.com/items";
            var newItem = new Product { Id = 1, Title = "New Item" };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.BadRequest,
                    Content = new StringContent("Bad request")
                });

            var result = await _callApiService.CreateItem(testUrl, newItem);

            // ensures the correct error message is returned when you get a BadRequest
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task EditItemValidItemReturnsSuccessResponse()
        {
            var testUrl = "https://api.example.com/Product/1";
            var updatedItem = new Product { Id = 1, Title = "Updated Product" };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Put &&
                        req.RequestUri.ToString() == testUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("Updated successfully")
                });

            var result = await _callApiService.EditItem(testUrl, updatedItem);

            // Tests updating product function
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task EditItemItemNotFoundReturnsNotFoundResponse()
        {
            var testUrl = "https://api.example.com/items/999";
            var updatedItem = new Product { Id = 999, Title = "Updated Item" };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Item not found")
                });

            var result = await _callApiService.EditItem(testUrl, updatedItem);

            // checks what happens when a product you updating dosen't exist
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task EditItemHttpClientThrowsExceptionWhenApiCantBeReached()
        {
            var testUrl = "https://api.example.com/items/1";
            var updatedItem = new Product { Id = 1, Title = "Updated Item" };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Looks to see if you get the correct error message when the api is down
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _callApiService.EditItem(testUrl, updatedItem));
        }

        [Fact]
        public async Task DeleteItemValidUrlReturnsSuccessResponse()
        {
            var testUrl = "https://api.example.com/items/1";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Delete &&
                        req.RequestUri.ToString() == testUrl),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NoContent
                });

            var result = await _callApiService.DeleteItem(testUrl);

            // Tests the delete item function
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NoContent, result.StatusCode);
        }

        [Fact]
        public async Task DeleteItemItemNotFoundReturnsNotFoundResponse()
        {
            var testUrl = "https://api.example.com/items/999";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound,
                    Content = new StringContent("Item not found")
                });

            var result = await _callApiService.DeleteItem(testUrl);

            // Tests what happens when you try to delete a product that doesn't exist
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task DeleteItemHttpClientThrowsExceptionWhenApiCantBeReached()
        {
            var testUrl = "https://api.example.com/items/1";

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            // Looks to see if you get the correct error message when the api is down
            await Assert.ThrowsAsync<HttpRequestException>(() =>
                _callApiService.DeleteItem(testUrl));
        }

        [Fact]
        public void ConstructorWithValidHttpClientFactoryCreatesInstance()
        {
            var mockFactory = new Mock<IHttpClientFactory>();

            var service = new CallApiService(mockFactory.Object);

            // Checks if the service is created correctly
            Assert.NotNull(service);
        }

        [Fact]
        public async Task AllMethods_UseCorrectHttpClientName()
        {
            var testUrl = "https://api.example.com/test";
            var testItem = new Product { Id = 1, Title = "Test" };

            _mockHttpMessageHandler.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("{\"id\": 1, \"title\": \"Test\"}")
                });

            await _callApiService.GetDataFromApi<Product>(testUrl);
            await _callApiService.CreateItem(testUrl, testItem);
            await _callApiService.EditItem(testUrl, testItem);
            await _callApiService.DeleteItem(testUrl);

            // Verify that CreateClient was called with the correct name
            _mockHttpClientFactory.Verify(x => x.CreateClient("MyCallApiService"), Times.Exactly(4));
        }

    }

}