using GrossistenApp.Models;
using GrossistenApp.Pages;
using GrossistenApp.Service;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrossitenTest
{
    public class SpecificProductDetailsTests
    {
        private readonly Mock<ICallApiService> _mockCallApiService;
        private readonly SpecificProductDetailsModel _pageModel;

        public SpecificProductDetailsTests()
        {
            _mockCallApiService = new Mock<ICallApiService>();
            _pageModel = new SpecificProductDetailsModel(_mockCallApiService.Object);
        }

        [Fact]
        public async Task ValidIdShowsSpecificProductDetails()
        {

            //set mock data to use for tests
            var productId = 1;
            var expectedProduct = new Product
            {
                Id = productId,
                Title = "Test Product",
                Description = "Test Description",
                Price = 100.1
            };

            //run this instead of calling the api so we can test the info instead of trying to reach the api everytime
            _mockCallApiService
                .Setup(x => x.GetDataFromApi<Product>($"Product/{productId}"))
                .ReturnsAsync(expectedProduct);

            await _pageModel.OnGetAsync(productId);


            //Checks the data so its the expected values 
            Assert.NotNull(_pageModel.SpecificProductDetails);
            Assert.Equal(expectedProduct.Id, _pageModel.SpecificProductDetails.Id);
            Assert.Equal(expectedProduct.Title, _pageModel.SpecificProductDetails.Title);
            Assert.Equal(expectedProduct.Description, _pageModel.SpecificProductDetails.Description);
            Assert.Equal(expectedProduct.Price, _pageModel.SpecificProductDetails.Price);

            //Checks that the update and delete objects are also set
            Assert.Equal(expectedProduct, _pageModel.UpdateSpecificProductDetails);
            Assert.Equal(expectedProduct, _pageModel.DeleteSpecificProductObject);
        }

        [Fact]
        public async Task ApiThrowsExceptionSetsErrorProduct()
        {
            var productId = 1;
            _mockCallApiService
                .Setup(x => x.GetDataFromApi<Product>($"Product/{productId}"))
                .ThrowsAsync(new HttpRequestException("API Error"));

            
            await _pageModel.OnGetAsync(productId);

            // makes sure you get the error message 
            Assert.NotNull(_pageModel.SpecificProductDetails);
            Assert.Equal("Kan inte hämta information testa igen.", _pageModel.SpecificProductDetails.Title);

            // Verify that the update and delete objects are also set to the error product
            Assert.Equal(_pageModel.SpecificProductDetails, _pageModel.UpdateSpecificProductDetails);
            Assert.Equal(_pageModel.SpecificProductDetails, _pageModel.DeleteSpecificProductObject);
        }

        [Fact]
        public async Task ValidProductUpdatesAndRedirects()
        {
            var productId = 1;
            var existingProduct = new Product
            {
                Id = productId,
                ArticleNumber = "1234567890",
                Title = "Test Title1",
                Description = "Test Title1",
                Size = "L",
                Price = 100.1,
                Category = "Test Category1",
                Quantity = 10
            };

            var updatedProduct = new Product
            {
                Id = productId,
                ArticleNumber = "123456789",
                Title = "Test Title2",
                Description = "Test Title2",
                Size = "M",
                Price = 140.1,
                Category = "Test Category2",
                Quantity = 20
            };

            _pageModel.UpdateSpecificProductDetails = updatedProduct;

            _mockCallApiService
                .Setup(x => x.GetDataFromApi<Product>($"Product/{productId}"))
                .ReturnsAsync(existingProduct);

            _mockCallApiService
                .Setup(x => x.EditItem($"Product/{productId}", It.IsAny<Product>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var result = await _pageModel.OnPostUpdateAsync();

            //checks that it redirects correctly when updating
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./SpecificProductDetails", redirectResult.PageName);
            Assert.Equal(productId, redirectResult.RouteValues?["id"]);

            // Verify the EditItem was called with updated values
            _mockCallApiService.Verify(x => x.EditItem($"Product/{productId}",
                It.Is<Product>(p =>
                    p.ArticleNumber == updatedProduct.ArticleNumber &&
                    p.Title == updatedProduct.Title &&
                    p.Description == updatedProduct.Description &&
                    p.Size == updatedProduct.Size &&
                    p.Price == updatedProduct.Price &&
                    p.Category == updatedProduct.Category &&
                    p.Quantity == updatedProduct.Quantity)), Times.Once);
        }

        [Fact]
        public async Task ProductNotFoundReturnsNotFound()
        {
            var productId = 1;
            _pageModel.UpdateSpecificProductDetails = new Product { Id = productId };

            _mockCallApiService
                .Setup(x => x.GetDataFromApi<Product?>($"Product/{productId}"))
                .ReturnsAsync((Product?)null);

            var result = await _pageModel.OnPostUpdateAsync();

            //checks that you get a notFound if the product dosen't exist
            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task ValidProductDeletesAndRedirects()
        {
            var productId = 1;
            var productToDelete = new Product { Id = productId, Title = "Product to Delete" };

            _pageModel.DeleteSpecificProductObject = new Product { Id = productId };

            _mockCallApiService
                .Setup(x => x.GetDataFromApi<Product>($"Product/{productId}"))
                .ReturnsAsync(productToDelete);

            _mockCallApiService
                .Setup(x => x.DeleteItem($"Product/{productId}"))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            
            var result = await _pageModel.OnPostDeleteAsync();

            //checks that it redirects correctly when updating
            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("/StockOverview", redirectResult.PageName);

            // Verify DeleteItem was called
            _mockCallApiService.Verify(x => x.DeleteItem($"Product/{productId}"), Times.Once);
        }

        [Fact]
        public async Task ApiCallSucceedsCallsEditItemCorrectly()
        {
            // Arrange
            var productId = 1;
            var existingProduct = new Product { Id = productId, Title = "Test Title1" };
            var updateData = new Product
            {
                Id = productId,
                ArticleNumber = "1234567890",
                Title = "Test Title2",
                Description = "Test Description",
                Size = "L",
                Price = 100.1,
                Category = "Test category",
                Quantity = 5
            };

            _pageModel.UpdateSpecificProductDetails = updateData;

            _mockCallApiService
                .Setup(x => x.GetDataFromApi<Product>($"Product/{productId}"))
                .ReturnsAsync(existingProduct);

            await _pageModel.OnPostUpdateAsync();

            // Calls api and then looks if it edited the product correctly
            _mockCallApiService.Verify(x => x.GetDataFromApi<Product>($"Product/{productId}"), Times.Once);
            _mockCallApiService.Verify(x => x.EditItem($"Product/{productId}",
                It.Is<Product>(p => p.Id == productId)), Times.Once);
        }
    }
}
