using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrossistenApp.Models;
using GrossistenApp.ViewModels;
using GrossistenApp.Service;

namespace GrossistenApp.Pages
{
    public class OutgoingProductsModel : PageModel
    {
        private readonly CallApiService _callApiService;
        public OutgoingProductsModel(CallApiService callApiService)
        {
            _callApiService = callApiService;
        }
        [BindProperty]
        public Product ProductObject { get; set; }

        public List<Product> ProductsFromDbList { get; set; }
        [BindProperty]
        public List<ProductInputViewModel> ProductsToAddFromInput { get; set; }
        [BindProperty]
        public Dictionary<int, int> ProductQuantitiesToAdd { get; set; } = new Dictionary<int, int>();

        public List<Receipt> ReceiptsFromDbList { get; set; }
        [BindProperty]
        public Receipt ReceiptObject { get; set; }


        public async Task OnGetAsync()
        {
            ProductsFromDbList = await _callApiService.GetDataFromApi<List<Product>>("Product");
            ReceiptsFromDbList = await _callApiService.GetDataFromApi<List<Receipt>>("Receipt");

        }

        public async Task<IActionResult> OnPostAddToStockAsync()
        {
            var allProductsFromDb = await _callApiService.GetDataFromApi<List<Product>>("Product");

            // Update quantities for products that have additions
            foreach (var product in ProductQuantitiesToAdd.Where(x => x.Value > 0))
            {
                var productId = product.Key;
                var quantityToAdd = product.Value;

                var specificProduct = allProductsFromDb.FirstOrDefault(p => p.Id == productId);
                if (specificProduct != null)
                {
                    specificProduct.Quantity = (specificProduct.Quantity ?? 0) - quantityToAdd;
                }
            }

            // Update products in bulk
            var toUpdate = allProductsFromDb
                .Where(p => ProductQuantitiesToAdd.ContainsKey(p.Id) && ProductQuantitiesToAdd[p.Id] > 0)
                .ToList();

            if (toUpdate.Any())
            {
                await _callApiService.EditItem("Product/bulk", toUpdate);
            }

            // Create Receipt
            ReceiptObject.WorkerName = "Svenne";
            ReceiptObject.showAsIncomingReceipt = false;
            ReceiptObject.showAsOutgoingReceipt = true;
            ReceiptObject.DateAndTimeCreated = DateTime.Now;

            await _callApiService.CreateItem("Receipt", ReceiptObject);

            // Add products to receipt
            var receipts = await _callApiService.GetDataFromApi<List<Receipt>>("Receipt");
            int highestReceiptIdInDb = receipts.Max(r => r.Id);

            foreach (var product in ProductQuantitiesToAdd.Where(x => x.Value > 0))
            {
                var productId = product.Key;
                var quantityToAdd = product.Value;

                var choosenProductToAdd = allProductsFromDb.FirstOrDefault(p => p.Id == productId);

                if (choosenProductToAdd != null)
                {
                    ProductObject.Id = 0;
                    ProductObject.ArticleNumber = choosenProductToAdd.ArticleNumber;
                    ProductObject.Title = choosenProductToAdd.Title;
                    ProductObject.Description = choosenProductToAdd.Description;
                    ProductObject.Size = choosenProductToAdd.Size;
                    ProductObject.Price = choosenProductToAdd.Price;
                    ProductObject.Category = choosenProductToAdd.Category;
                    ProductObject.Quantity = quantityToAdd;
                    ProductObject.ReceiptId = highestReceiptIdInDb;
                    ProductObject.ShowInAvailableToPurchase = false;
                    ProductObject.ShowInStock = false;
                    ProductObject.ShowOnReceipt = true;

                    await _callApiService.CreateItem("Product", ProductObject);
                }
            }

            return RedirectToPage("./OutgoingProducts");
        }
    }
}
