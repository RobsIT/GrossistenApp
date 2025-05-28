using GrossistenApp.Models;
using GrossistenApp.Pages;
using GrossistenApp.Service;
using GrossistenApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace GrossitenTest
{
    public class OutgoingProductsModelTests
    {
        private readonly Mock<ICallApiService> _mockCallApiService;
        private readonly OutgoingProductsModel _pageModel;

        public OutgoingProductsModelTests()
        {
            _mockCallApiService = new Mock<ICallApiService>();
            _pageModel = new OutgoingProductsModel(_mockCallApiService.Object);
        }

        [Fact]
        public async Task WhenProductsApiSucceedsDataSentToInProductLists()
        {
            // Make mock data to use for tests
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", ShowInStock = true, ShowOnReceipt = false },
                new Product { Id = 2, Title = "Product 2", ShowInStock = false, ShowOnReceipt = true },
                new Product { Id = 3, Title = "Product 3", ShowInStock = true, ShowOnReceipt = true }
            };

            var mockReceipts = new List<Receipt>
            {
                new Receipt { Id = 1, WorkerName = "Worker 1", showAsOutgoingReceipt = true },
                new Receipt { Id = 2, WorkerName = "Worker 2", showAsOutgoingReceipt = false }
            };

            // replaces api calls so tests don't call the api when testing
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(mockReceipts);

            //runs the OnGetAsync from outgoingProducts
            await _pageModel.OnGetAsync();

            // Testing if 2 products in OutgoingProductsList and they are true
            Assert.Equal(2, _pageModel.OutgoingProductsFromDbList.Count);
            Assert.All(_pageModel.OutgoingProductsFromDbList, p => Assert.True(p.ShowInStock));

            // Testing if 2 products in ProductsOnReceipts and they are true
            Assert.Equal(2, _pageModel.ProductsFromDbListOnReceipt.Count);
            Assert.All(_pageModel.ProductsFromDbListOnReceipt, p => Assert.True(p.ShowOnReceipt));

            // Testing if 1 receipts exist and are true 
            Assert.Single(_pageModel.OutgoingReceiptsFromDbList);
            Assert.All(_pageModel.OutgoingReceiptsFromDbList, r => Assert.True(r.showAsOutgoingReceipt));
        }

        [Fact]
        public async Task WhenProductsApiFailsShouldReturnErrorProduct()
        {

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ThrowsAsync(new Exception("API Error"));
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(new List<Receipt>());

            await _pageModel.OnGetAsync();

            // Checks when api fails you get 1 product with error info and only 1 and ShowInStock is true
            Assert.Single(_pageModel.OutgoingProductsFromDbList);
            Assert.Contains("Kunde inte hämta information", _pageModel.OutgoingProductsFromDbList.First().Title);
            Assert.True(_pageModel.OutgoingProductsFromDbList.First().ShowInStock);
        }

        [Fact]
        public async Task WhenReceiptsApiFailsShouldReturnErrorReceipt()
        {
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(new List<Product>());
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ThrowsAsync(new Exception("API Error"));

 
            await _pageModel.OnGetAsync();

            // Checks when api fails you get 1 receipt with error info and only 1 and ShowInStock is true
            Assert.Single(_pageModel.OutgoingReceiptsFromDbList);
            Assert.Contains("Kunde inte hämta information", _pageModel.OutgoingReceiptsFromDbList.First().WorkerName);
            Assert.True(_pageModel.OutgoingReceiptsFromDbList.First().showAsOutgoingReceipt);
        }

        [Fact]
        public async Task WhenNullShowInStockValuesShouldFilterCorrectly()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", ShowInStock = true },
                new Product { Id = 2, Title = "Product 2", ShowInStock = null },
                new Product { Id = 3, Title = "Product 3", ShowInStock = false }
            };

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(new List<Receipt>());

            await _pageModel.OnGetAsync();

            // Testing filtering so null dosen't go to the wrong place
            Assert.Single(_pageModel.OutgoingProductsFromDbList);
            Assert.Equal("Product 1", _pageModel.OutgoingProductsFromDbList.First().Title);
        }

        [Fact]
        public async Task ValidInputShouldUpdateQuantitiesAndCreateReceipt()
        {
            var mockProducts = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Title = "Test Title1 ",
                    ArticleNumber = "1234567890",
                    Description = "Test Description",
                    Size = "L",
                    Price = 100.1,
                    Category = "Test Category",
                    Quantity = 10
                },
                new Product
                {
                    Id = 2,
                    Title = "Test Title2",
                    ArticleNumber = "2345678901",
                    Quantity = 5
                }
            };

            var mockReceipts = new List<Receipt>
            {
                new Receipt { Id = 1 },
                new Receipt { Id = 2 }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 1, QuantityToAdd = 3 },
                new ProductInputViewModel { ProductId = 2, QuantityToAdd = 2 }
            };

            _pageModel.ReceiptObject = new Receipt();

            _pageModel.ProductObject = new Product();

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(mockReceipts);
            _mockCallApiService.Setup(x => x.EditItem("Product/bulk", It.IsAny<List<Product>>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            _mockCallApiService.Setup(x => x.CreateItem("Receipt", It.IsAny<Receipt>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created));
            _mockCallApiService.Setup(x => x.CreateItem("Product", It.IsAny<Product>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created));

            var result = await _pageModel.OnPostTakeFromStockAsync();

            //make sure it redirects correctly
            Assert.IsType<RedirectToPageResult>(result);
            var redirectResult = result as RedirectToPageResult;
            Assert.Equal("./OutgoingProducts", redirectResult.PageName);

            // Verify product quantities were updated
            Assert.Equal(7, mockProducts.First(p => p.Id == 1).Quantity);
            Assert.Equal(3, mockProducts.First(p => p.Id == 2).Quantity);

            // Verify API calls
            _mockCallApiService.Verify(x => x.EditItem("Product/bulk", It.IsAny<List<Product>>()), Times.Once);
            _mockCallApiService.Verify(x => x.CreateItem("Receipt", It.IsAny<Receipt>()), Times.Once);
            _mockCallApiService.Verify(x => x.CreateItem("Product", It.IsAny<Product>()), Times.Exactly(2));
        }

        [Fact]
        public async Task WithInsufficientStockShouldNotUpdateProduct()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Quantity = 2 }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 1, QuantityToAdd = 5 } // More than available
            };

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);

            var result = await _pageModel.OnPostTakeFromStockAsync();

            //make sure it redirects correctly
            Assert.IsType<RedirectToPageResult>(result);

            // Verify quantity was not changed
            Assert.Equal(2, mockProducts.First().Quantity);

            // Verify no API calls were made for updates
            _mockCallApiService.Verify(x => x.EditItem(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            _mockCallApiService.Verify(x => x.CreateItem(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task WithZeroQuantityShouldNotUpdateProduct()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Quantity = 10 }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 1, QuantityToAdd = 0 }
            };

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);

            var result = await _pageModel.OnPostTakeFromStockAsync();

            //make sure it redirects correctly
            Assert.IsType<RedirectToPageResult>(result);

            // Verify quantity was not changed
            Assert.Equal(10, mockProducts.First().Quantity);

            // Verify no API calls were made for updates
            _mockCallApiService.Verify(x => x.EditItem(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            _mockCallApiService.Verify(x => x.CreateItem(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task WithNonExistentProductShouldNotUpdateAnything()
        {

            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Quantity = 10 }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 999, QuantityToAdd = 5 } // Non-existent product
            };

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);

            var result = await _pageModel.OnPostTakeFromStockAsync();

            _pageModel.ProductObject = new Product();

            Assert.IsType<RedirectToPageResult>(result);

            // Verify no API calls were made for updates
            _mockCallApiService.Verify(x => x.EditItem(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
            _mockCallApiService.Verify(x => x.CreateItem(It.IsAny<string>(), It.IsAny<object>()), Times.Never);
        }

        [Fact]
        public async Task ShouldCreateReceiptWithCorrectProps()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Quantity = 10 }
            };

            var mockReceipts = new List<Receipt>
            {
                new Receipt { Id = 5 }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 1, QuantityToAdd = 3 }
            };

            _pageModel.ReceiptObject = new Receipt();

            _pageModel.ProductObject = new Product();

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(mockReceipts);
            _mockCallApiService.Setup(x => x.EditItem("Product/bulk", It.IsAny<List<Product>>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            _mockCallApiService.Setup(x => x.CreateItem("Receipt", It.IsAny<Receipt>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created));
            _mockCallApiService.Setup(x => x.CreateItem("Product", It.IsAny<Product>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created));

            await _pageModel.OnPostTakeFromStockAsync();

            // test making a receipt and see if it get the correct properties
            _mockCallApiService.Verify(x => x.CreateItem("Receipt", It.Is<Receipt>(r =>
                r.WorkerName == "Svenne" &&
                r.showAsIncomingReceipt == false &&
                r.showAsOutgoingReceipt == true &&
                r.DateAndTimeCreated != default(DateTime)
            )), Times.Once);
        }

        [Fact]
        public async Task ShouldCreateProductsWithCorrectReceiptId()
        {
            // Arrange
            var mockProducts = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    Title = "Test Title",
                    ArticleNumber = "1234567890",
                    Description = "Test Description",
                    Size = "L",
                    Price = 100.1,
                    Category = "Test Category",
                    Quantity = 10
                }
            };

            var mockReceipts = new List<Receipt>
            {
                new Receipt { Id = 3 },
                new Receipt { Id = 7 },
                new Receipt { Id = 5 }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 1, QuantityToAdd = 2 }
            };

            _pageModel.ReceiptObject = new Receipt();
            _pageModel.ProductObject = new Product();

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(mockReceipts);
            _mockCallApiService.Setup(x => x.EditItem("Product/bulk", It.IsAny<List<Product>>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            _mockCallApiService.Setup(x => x.CreateItem("Receipt", It.IsAny<Receipt>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created));
            _mockCallApiService.Setup(x => x.CreateItem("Product", It.IsAny<Product>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created));

            await _pageModel.OnPostTakeFromStockAsync();

            // test making a product and it chooses the correct receipt, the latest of them
            _mockCallApiService.Verify(x => x.CreateItem("Product", It.Is<Product>(p =>
                p.Id == 0 &&
                p.ReceiptId == 7 && // Should use highest receipt ID
                p.Quantity == 2 &&
                p.ShowInAvailableToPurchase == false &&
                p.ShowInStock == false &&
                p.ShowOnReceipt == true
            )), Times.Once);
        }

        [Fact]
        public async Task WithMixedValidAndInvalidInputsShouldOnlyAcceptValid()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Quantity = 10 },
                new Product { Id = 2, Title = "Product 2", Quantity = 3 }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 1, QuantityToAdd = 5 }, // Valid
                new ProductInputViewModel { ProductId = 2, QuantityToAdd = 10 }, // Invalid - too much
                new ProductInputViewModel { ProductId = 999, QuantityToAdd = 2 } // Invalid - doesn't exist
            };

            _pageModel.ReceiptObject = new Receipt();

            _pageModel.ProductObject = new Product();

            var mockReceipts = new List<Receipt> { new Receipt { Id = 1 } };

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(mockReceipts);
            _mockCallApiService.Setup(x => x.EditItem("Product/bulk", It.IsAny<List<Product>>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            _mockCallApiService.Setup(x => x.CreateItem("Receipt", It.IsAny<Receipt>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created));
            _mockCallApiService.Setup(x => x.CreateItem("Product", It.IsAny<Product>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.Created));

            await _pageModel.OnPostTakeFromStockAsync();



            // Only one product should be updated
            _mockCallApiService.Verify(x => x.EditItem("Product/bulk", It.Is<List<Product>>(list => list.Count == 1)), Times.Once);
            _mockCallApiService.Verify(x => x.CreateItem("Product", It.IsAny<Product>()), Times.Once);

            // Verify the correct product was updated
            Assert.Equal(5, mockProducts.First(p => p.Id == 1).Quantity);
            Assert.Equal(3, mockProducts.First(p => p.Id == 2).Quantity);
        }
    }
}