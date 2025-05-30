using GrossistenApp.Models;
using GrossistenApp.Pages;
using GrossistenApp.Service;
using GrossistenApp.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GrossistenTest
{
    public class IncomingProductsTests
    {
        private readonly Mock<ICallApiService> _mockCallApiService;
        private readonly IncomingProductsModel _pageModel;

        public IncomingProductsTests()
        {
            _mockCallApiService = new Mock<ICallApiService>();
            _pageModel = new IncomingProductsModel(_mockCallApiService.Object);
        }

        [Fact]
        public async Task ShouldAddProductsToIncomingProductsFromDbListWhenApiCallSucceeds()
        {
            //setup mock data 
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", ShowInAvailableToPurchase = true },
                new Product { Id = 2, Title = "Product 2", ShowInAvailableToPurchase = false },
                new Product { Id = 3, Title = "Product 3", ShowInAvailableToPurchase = true }
            };

            var mockReceipts = new List<Receipt>
            {
                new Receipt { Id = 1, WorkerName = "Worker 1", showAsIncomingReceipt = true },
                new Receipt { Id = 2, WorkerName = "Worker 2", showAsIncomingReceipt = false }
            };

            //sets up the mock data to return instead of call the api
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(mockReceipts);

            //calls the page to test it
            await _pageModel.OnGetAsync();

            //checks if the products in IncomingProductsFromDbList is 2 
            Assert.Equal(2, _pageModel.IncomingProductsFromDbList.Count);
            Assert.All(_pageModel.IncomingProductsFromDbList, p => Assert.True(p.ShowInAvailableToPurchase));
            Assert.Single(_pageModel.IncomingReceiptsFromDbList);
            Assert.True(_pageModel.IncomingReceiptsFromDbList.First().showAsIncomingReceipt);
        }

        [Fact]
        public async Task ShouldAddProductsToProductsFromDbListOnReceipt_WhenApiCallSucceeds()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", ShowOnReceipt = true },
                new Product { Id = 2, Title = "Product 2", ShowOnReceipt = false },
                new Product { Id = 3, Title = "Product 3", ShowOnReceipt = true }
            };

            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(new List<Receipt>());

            await _pageModel.OnGetAsync();

            //checks if the ProductsFromDbListOnReceipt has 2 products and if they have ShowOnreceipts as true
            Assert.Equal(2, _pageModel.ProductsFromDbListOnReceipt.Count);
            Assert.All(_pageModel.ProductsFromDbListOnReceipt, p => Assert.True(p.ShowOnReceipt));
        }

        [Fact]
        public async Task ShouldHandleProductApiException_AndReturnErrorMessage()
        {
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ThrowsAsync(new Exception("API Error"));
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(new List<Receipt>());

            await _pageModel.OnGetAsync();

            // checks to see if i get a error message when the api can't send the data.
            Assert.Single(_pageModel.IncomingProductsFromDbList);
            Assert.Contains("Kunde inte hämta information", _pageModel.IncomingProductsFromDbList.First().Title);
        }

        [Fact]
        public async Task ShouldHandleReceiptApiException_AndReturnErrorMessage()
        {
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(new List<Product>());
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ThrowsAsync(new Exception("API Error"));

            await _pageModel.OnGetAsync();

            // checks to see if i get a error message when the api can't send the data.
            Assert.Single(_pageModel.IncomingReceiptsFromDbList);
            Assert.Contains("Kunde inte hämta information", _pageModel.IncomingReceiptsFromDbList.First().WorkerName);
        }

        [Fact]
        public async Task ShouldSetCorrectProductPropsAndCallCreateItem()
        {
            _pageModel.ProductObject = new Product
            {
                Title = "Test Product",
                Description = "Test Description"
            };

            _mockCallApiService.Setup(x => x.CreateItem("Product", It.IsAny<Product>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var result = await _pageModel.OnPostCreateProductAsync();

            // checks if when creating a product the quantity is 0 and it dosen't have a receiptId,
            //if showInAvaiable is true and its not in showInstock and showinReceipt
            Assert.Equal(0, _pageModel.ProductObject.Quantity);
            Assert.Equal(0, _pageModel.ProductObject.ReceiptId);
            Assert.True(_pageModel.ProductObject.ShowInAvailableToPurchase);
            Assert.False(_pageModel.ProductObject.ShowInStock);
            Assert.False(_pageModel.ProductObject.ShowOnReceipt);

            _mockCallApiService.Verify(x => x.CreateItem("Product", _pageModel.ProductObject), Times.Once);

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./IncomingProducts", redirectResult.PageName);
        }

        [Fact]
        public async Task ShouldUpdateProductQuantitiesAndCreateReceipt()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Quantity = 10, ShowInStock = true },
                new Product { Id = 2, Title = "Product 2", Quantity = 0, ShowInStock = false }
            };

            var mockReceipts = new List<Receipt>
            {
                new Receipt { Id = 1, WorkerName = "Worker 1" },
                new Receipt { Id = 2, WorkerName = "Worker 2" }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 1, QuantityToAdd = 5 },
                new ProductInputViewModel { ProductId = 2, QuantityToAdd = 3 }
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
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            _mockCallApiService.Setup(x => x.CreateItem("Product", It.IsAny<Product>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var result = await _pageModel.OnPostAddToStockAsync();

            // Verify product quantities were updated
            Assert.Equal(15, mockProducts.First(p => p.Id == 1).Quantity);
            Assert.Equal(3, mockProducts.First(p => p.Id == 2).Quantity);

            // Verify ShowInStock was updated
            Assert.True(mockProducts.First(p => p.Id == 1).ShowInStock);
            Assert.True(mockProducts.First(p => p.Id == 2).ShowInStock);

            // Verify receipt properties
            Assert.Equal("Svenne", _pageModel.ReceiptObject.WorkerName);
            Assert.True(_pageModel.ReceiptObject.showAsIncomingReceipt);
            Assert.False(_pageModel.ReceiptObject.showAsOutgoingReceipt);
            Assert.True(_pageModel.ReceiptObject.DateAndTimeCreated <= DateTime.Now);

            // Verify API calls
            _mockCallApiService.Verify(x => x.GetDataFromApi<List<Product>>("Product"), Times.Once);
            _mockCallApiService.Verify(x => x.EditItem("Product/bulk", It.IsAny<List<Product>>()), Times.Once);
            _mockCallApiService.Verify(x => x.CreateItem("Receipt", It.IsAny<Receipt>()), Times.Once);
            _mockCallApiService.Verify(x => x.GetDataFromApi<List<Receipt>>("Receipt"), Times.Once);
            _mockCallApiService.Verify(x => x.CreateItem("Product", It.IsAny<Product>()), Times.Exactly(2));

            var redirectResult = Assert.IsType<RedirectToPageResult>(result);
            Assert.Equal("./IncomingProducts", redirectResult.PageName);
        }

        [Fact]
        public async Task ShouldSkipProductsWithZeroQuantity()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Quantity = 10 }
            };

            var mockReceipts = new List<Receipt>
            {
                new Receipt { Id = 1, WorkerName = "Worker 1" }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 1, QuantityToAdd = 0 }
            };

            _pageModel.ReceiptObject = new Receipt();


            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Product>>("Product"))
                .ReturnsAsync(mockProducts);
            _mockCallApiService.Setup(x => x.GetDataFromApi<List<Receipt>>("Receipt"))
                .ReturnsAsync(mockReceipts);
            _mockCallApiService.Setup(x => x.EditItem("Product/bulk", It.IsAny<List<Product>>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            _mockCallApiService.Setup(x => x.CreateItem("Receipt", It.IsAny<Receipt>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            var result = await _pageModel.OnPostAddToStockAsync();

            // Verify product quantity wasn't changed
            Assert.Equal(10, mockProducts.First().Quantity);

            // Verify CreateItem for Product was not called for 0 quantity items
            _mockCallApiService.Verify(x => x.CreateItem("Product", It.IsAny<Product>()), Times.Never);
        }

        [Fact]
        public async Task ShouldCreateReceiptProductsWithCorrectProps()
        {
            // Arrange
            var mockProducts = new List<Product>
            {
                new Product
                {
                    Id = 1,
                    ArticleNumber = "1234567890",
                    Title = "Test product",
                    Description = "Test description",
                    Size = "L",
                    Price = 100.1,
                    Category = "Test category",
                    Quantity = 10
                }
            };

            var mockReceipts = new List<Receipt>
            {
                new Receipt { Id = 5, WorkerName = "Test person" }
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
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            _mockCallApiService.Setup(x => x.CreateItem("Product", It.IsAny<Product>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            await _pageModel.OnPostAddToStockAsync();

            // Checks to see if the product was created correctly and no values is wrong
            _mockCallApiService.Verify(x => x.CreateItem("Product", It.Is<Product>(p =>
                p.Id == 0 &&
                p.ArticleNumber == "1234567890" &&
                p.Title == "Test product" &&
                p.Description == "Test description" &&
                p.Size == "L" &&
                p.Price == 100.1 &&
                p.Category == "Test category" &&
                p.Quantity == 3 &&
                p.ReceiptId == 5 &&
                p.ShowInAvailableToPurchase == false &&
                p.ShowInStock == false &&
                p.ShowOnReceipt == true
            )), Times.Once);
        }

        [Fact]
        public async Task ShouldHandleMultipleProductUpdates()
        {
            var mockProducts = new List<Product>
            {
                new Product { Id = 1, Title = "Product 1", Quantity = 5, ShowInStock = true },
                new Product { Id = 2, Title = "Product 2", Quantity = 0, ShowInStock = false },
                new Product { Id = 3, Title = "Product 3", Quantity = 2, ShowInStock = true }
            };

            var mockReceipts = new List<Receipt>
            {
                new Receipt { Id = 10, WorkerName = "Worker 1" }
            };

            _pageModel.ProductsToAddFromInput = new List<ProductInputViewModel>
            {
                new ProductInputViewModel { ProductId = 1, QuantityToAdd = 2 },
                new ProductInputViewModel { ProductId = 2, QuantityToAdd = 5 },
                new ProductInputViewModel { ProductId = 3, QuantityToAdd = 0 }
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
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));
            _mockCallApiService.Setup(x => x.CreateItem("Product", It.IsAny<Product>()))
                .ReturnsAsync(new HttpResponseMessage(System.Net.HttpStatusCode.OK));

            await _pageModel.OnPostAddToStockAsync();

            // checks so they have the correct quantity 
            Assert.Equal(7, mockProducts.First(p => p.Id == 1).Quantity);
            Assert.Equal(5, mockProducts.First(p => p.Id == 2).Quantity);
            Assert.Equal(2, mockProducts.First(p => p.Id == 3).Quantity);

            // checks to see so all products with stock is true 
            Assert.True(mockProducts.First(p => p.Id == 1).ShowInStock);
            Assert.True(mockProducts.First(p => p.Id == 2).ShowInStock);
            Assert.True(mockProducts.First(p => p.Id == 3).ShowInStock);

            // Should create receipt products only for items with quantity over 0
            _mockCallApiService.Verify(x => x.CreateItem("Product", It.IsAny<Product>()), Times.Exactly(2));
        }

    }
}
