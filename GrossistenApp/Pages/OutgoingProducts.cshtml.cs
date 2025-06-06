using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrossistenApp.Models;
using GrossistenApp.ViewModels;
using GrossistenApp.Service;
using Microsoft.CodeAnalysis;

namespace GrossistenApp.Pages
{
    public class OutgoingProductsModel : PageModel
    {
        private readonly ICallApiService _callApiService;
        public OutgoingProductsModel(ICallApiService callApiService)
        {
            _callApiService = callApiService;
        }
       
        [BindProperty]
        public Product ProductObject { get; set; }

        public List<Product> ProductsFromDbList { get; set; }
        public List<Product> OutgoingProductsFromDbList { get; set; }
        public List<Product> ProductsFromDbListOnReceipt { get; set; }
        
        [BindProperty]
        public List<ProductInputViewModel> ProductsToAddFromInput { get; set; }

        public List<Receipt> OutgoingReceiptsFromDbList { get; set; }
        
        [BindProperty]
        public Receipt ReceiptObject { get; set; }

        public async Task OnGetAsync()
        {
            List<Product> allProductsFromDbList;

            try
            {
                allProductsFromDbList = await _callApiService.GetDataFromApi<List<Product>>("Product");
            }
            catch(Exception)
            {
                allProductsFromDbList = new List<Product>
                {
                    new Product { Title = "Kunde inte hämta information, testa igen senare(Starta Api).", ShowInStock = true}
                };
            }
            
            OutgoingProductsFromDbList = allProductsFromDbList.Where(p => p.ShowInStock ?? false).OrderByDescending(p => p.Id).ToList();
            ProductsFromDbListOnReceipt = allProductsFromDbList.Where(p => p.ShowOnReceipt ?? false).ToList();

            List<Receipt> allReceiptsFromDbList;

            try
            {
                allReceiptsFromDbList = await _callApiService.GetDataFromApi<List<Receipt>>("Receipt");
            }
            catch(Exception)
            {
                allReceiptsFromDbList = new List<Receipt>
                { 
                    new Receipt { WorkerName = "Kunde inte hämta information", showAsOutgoingReceipt = true }
                };
            }

            OutgoingReceiptsFromDbList = allReceiptsFromDbList.Where(r => r.showAsOutgoingReceipt ?? false).OrderByDescending(r => r.DateAndTimeCreated).ToList();
        }

        public async Task<IActionResult> OnPostTakeFromStockAsync()
        {
            var allProductsFromDb = await _callApiService.GetDataFromApi<List<Product>>("Product");
            var productsAddedToCount = new List<Product>();
            var successfullyProcessedInputs = new List<ProductInputViewModel>();
            
            // Update quantities for products that have additions
            foreach (var product in ProductsToAddFromInput)
            {
                var specificProduct = allProductsFromDb.FirstOrDefault(p => p.Id == product.ProductId);

                if (product.QuantityToAdd > 0 && specificProduct != null && product.QuantityToAdd <= specificProduct.Quantity)
                {
                    specificProduct.Quantity = (specificProduct.Quantity ?? 0) - product.QuantityToAdd;
                    productsAddedToCount.Add(specificProduct);
                    successfullyProcessedInputs.Add(product);
                }
            }

            if (productsAddedToCount.Count > 0)
            {
                // Update products in bulk
                await _callApiService.EditItem("Product/bulk", productsAddedToCount);

                // Create Receipt
                ReceiptObject.WorkerName = "Sven";
                ReceiptObject.showAsIncomingReceipt = false;
                ReceiptObject.showAsOutgoingReceipt = true;
                ReceiptObject.DateAndTimeCreated = DateTime.Now;

                await _callApiService.CreateItem("Receipt", ReceiptObject);

                // Add products to receipt - ONLY for successfully processed items
                var receipts = await _callApiService.GetDataFromApi<List<Receipt>>("Receipt");
                int highestReceiptIdInDb = receipts.Max(r => r.Id);

                foreach (var product in successfullyProcessedInputs)
                {
                    var choosenProductToAdd = allProductsFromDb.FirstOrDefault(p => p.Id == product.ProductId);

                    if (choosenProductToAdd != null)
                    {
                        ProductObject.Id = 0;
                        ProductObject.ArticleNumber = choosenProductToAdd.ArticleNumber;
                        ProductObject.Title = choosenProductToAdd.Title;
                        ProductObject.Description = choosenProductToAdd.Description;
                        ProductObject.Size = choosenProductToAdd.Size;
                        ProductObject.Price = choosenProductToAdd.Price;
                        ProductObject.Category = choosenProductToAdd.Category;
                        ProductObject.Quantity = product.QuantityToAdd;
                        ProductObject.ReceiptId = highestReceiptIdInDb;
                        ProductObject.ShowInAvailableToPurchase = false;
                        ProductObject.ShowInStock = false;
                        ProductObject.ShowOnReceipt = true;

                        await _callApiService.CreateItem("Product", ProductObject);
                    }
                }
            }

            return RedirectToPage("./OutgoingProducts");
        }
    }
}
