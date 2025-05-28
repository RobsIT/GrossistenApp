using Moq;
using GrossistenApp.Pages;
using GrossistenApp.Models;
using GrossistenApp.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrossitenTest
{
    public class StockOverviewTests
    {
        private readonly Mock<ICallApiService> _mockCallApiService;
        private readonly StockOverviewModel _stockOverviewModel;

        public StockOverviewTests()
        {
            _mockCallApiService = new Mock<ICallApiService>();
            _stockOverviewModel = new StockOverviewModel(_mockCallApiService.Object);
        }

        [Fact]
        public void IfProductsFromDbListStartAsEmptyAndNotNull()
        {
            //checks if its null and empty
            Assert.NotNull(_stockOverviewModel.ProductsFromDbList);
            Assert.Empty(_stockOverviewModel.ProductsFromDbList);
        }

        [Fact]
        public async Task IfStockOverviewProductsFromDbListOnlyAddShowInStockTrue()
        {

            //make mock data to use for the test
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", ShowInStock = true },
                new Product { Id = 2, Title = "Product 2", ShowInStock = false },
                new Product { Id = 3, Title = "Product 3", ShowInStock = true },
                new Product { Id = 4, Title = "Product 4", ShowInStock = null }
            };

            //using mock and interface to set up mock data when you run the api call instead of calling the api
            _mockCallApiService
                .Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);

            //awaits for the mock data
            await _stockOverviewModel.OnGetAsync();

            //checks if its null, how many products that should be in the list, 
            //and last runs to check if all the items added has the ShowInstock true
            Assert.NotNull(_stockOverviewModel.StockOverviwProductsFromDbList);
            Assert.Equal(2, _stockOverviewModel.StockOverviwProductsFromDbList.Count);
            Assert.All(_stockOverviewModel.StockOverviwProductsFromDbList,
                product => Assert.True(product.ShowInStock));

            //then verify the correct products is in the correct places
            var productIds = _stockOverviewModel.StockOverviwProductsFromDbList.Select(p => p.Id).ToList();
            Assert.Contains(1, productIds);
            Assert.Contains(3, productIds);
        }

        [Fact]
        public async Task ReturnEmptyListWhenNoProductsHaveShowInStockTrue()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", ShowInStock = false },
                new Product { Id = 2, Title = "Product 2", ShowInStock = null },
                new Product { Id = 3, Title = "Product 3", ShowInStock = false }
            };

            _mockCallApiService
                .Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);

            // Act
            await _stockOverviewModel.OnGetAsync();

            // Checks to see if the any items was added when none should, and is not null.
            Assert.NotNull(_stockOverviewModel.StockOverviwProductsFromDbList);
            Assert.Empty(_stockOverviewModel.StockOverviwProductsFromDbList);
        }

        [Fact]
        public async Task ShouldHandleNullShowInStockValues()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", ShowInStock = null },
                new Product { Id = 2, Title = "Product 2", ShowInStock = true }
            };

            _mockCallApiService
                .Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);

            await _stockOverviewModel.OnGetAsync();

            // checks to see if null dosent get and the only product added is the one with id 2 as its true.
            Assert.Single(_stockOverviewModel.StockOverviwProductsFromDbList);
            Assert.Equal(2, _stockOverviewModel.StockOverviwProductsFromDbList.First().Id);
        }

        [Fact]
        public async Task ShouldCreateErrorProductWhenApiCallThrowsException()
        {
            _mockCallApiService
                .Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ThrowsAsync(new Exception("API Error"));

            await _stockOverviewModel.OnGetAsync();

            //checks to see if the list has 1 product and is not null
            Assert.NotNull(_stockOverviewModel.StockOverviwProductsFromDbList);
            Assert.Single(_stockOverviewModel.StockOverviwProductsFromDbList);

            //checks what the title of the first product in the list with the correct title and it has ShowInstock 
            var errorProduct = _stockOverviewModel.StockOverviwProductsFromDbList.First();
            Assert.Equal("Kunde inte hämta information, testa igen senare(Starta Api).", errorProduct.Title);
            Assert.True(errorProduct.ShowInStock);
        }

        [Fact]
        public async Task ShouldKeepProductData()
        {   
            var mockProducts = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    ArticleNumber = "1234567890",
                    Title = "Test Product",
                    Description = "Test Description",
                    Size = "L",
                    Price = 100.1,
                    Category = "Test Category",
                    Quantity = 99,
                    ShowInAvailableToPurchase = false,
                    ShowInStock = true,
                    ShowOnReceipt = null,
                    ReceiptId = null,
                }
            };

            _mockCallApiService
                .Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);

            await _stockOverviewModel.OnGetAsync();

            //checks to see so all the data is the same so nothing changes in when it moves around in the lists 
            var resultProduct = _stockOverviewModel.StockOverviwProductsFromDbList.First();
            Assert.Equal(1, resultProduct.Id);
            Assert.Equal("1234567890", resultProduct.ArticleNumber);
            Assert.Equal("Test Product", resultProduct.Title);
            Assert.Equal("L", resultProduct.Size);
            Assert.Equal(100.1, resultProduct.Price);
            Assert.Equal("Test Category", resultProduct.Category);
            Assert.Equal(99, resultProduct.Quantity);
            Assert.False(resultProduct.ShowInAvailableToPurchase);
            Assert.True(resultProduct.ShowInStock);
            Assert.Null(resultProduct.ShowOnReceipt);
            Assert.Null(resultProduct.ReceiptId);
        }
    }
}