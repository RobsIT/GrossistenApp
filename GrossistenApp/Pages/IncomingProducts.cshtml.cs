using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrossistenApp.Models;
using GrossistenApp.ViewModels;
using GrossistenApp.Service;

namespace GrossistenApp.Pages
{
    public class IncomingProductsModel : PageModel
    {
        private readonly CallApiService _callApiService;
        public IncomingProductsModel(CallApiService callApiService)
        {
            _callApiService = callApiService;
        }
        [BindProperty]
        public Product ProductObject { get; set; }
       
        public List<Product> ProductsFromDbList { get; set; }
        [BindProperty]
        public List<ProductInputViewModel> ProductsToAddFromInput { get; set; }
       
        public List<Receipt> ReceiptsFromDbList { get; set; }
        [BindProperty]
        public Receipt ReceiptObject { get; set; }


        public async Task OnGetAsync()
        {
            ProductsFromDbList = await _callApiService.GetDataFromApi<List<Product>>("Product");
            ReceiptsFromDbList = await _callApiService.GetDataFromApi<List<Receipt>>("Receipt");

        }

        //Form 1 in View
        public async Task<IActionResult> OnPostCreateProductAsync()
        {
            ProductObject.Quantity = 0;
            ProductObject.ReceiptId = 0;
            ProductObject.ShowInAvailableToPurchase = true;
            ProductObject.ShowInStock = false;
            ProductObject.ShowOnReceipt = false;
            await _callApiService.CreateItem("Product", ProductObject);
            return RedirectToPage("./IncomingProducts");
        }

        //Form 2 in View
        public async Task<IActionResult> OnPostAddToStockAsync()
        {

            var allProductsFromDb = await _callApiService.GetDataFromApi<List<Product>>("Product");

            //----Öka antal från Beställningsbara Produkter formuläret på befintlig produkt------
            foreach (var inputObject in ProductsToAddFromInput)
            {
                if (inputObject.QuantityToAdd > 0)
                {
                    var specificProduct = allProductsFromDb.FirstOrDefault(p => p.Id == inputObject.ProductId);
                    if (specificProduct != null)
                    {
                        
                        specificProduct.Quantity = specificProduct.Quantity + inputObject.QuantityToAdd;

                        // Uppdatera showInStock boolen om lagret är mer än antal 0. Sql-Db läser true/faulse som 1/0.
                        specificProduct.ShowInStock = specificProduct.Quantity > 0;
                    }
                
                }
            }

           await _callApiService.EditItem("Product/bulk", allProductsFromDb);

            //-------Create Receipt---------------
            ReceiptObject.WorkerName = "Svenne";
            ReceiptObject.showAsIncomingReceipt = true;
            ReceiptObject.showAsOutgoingReceipt = false;
            ReceiptObject.DateAndTimeCreated = DateTime.Now;

            await _callApiService.CreateItem("Receipt", ReceiptObject);

            //Lägg till dom ökade Produkterna till kvittot
            int highestReceiptIdInDb = _callApiService.GetDataFromApi<List<Receipt>>("Receipt").Result.Max(r => r.Id);
            foreach (var inputObject in ProductsToAddFromInput)
            {

                if (inputObject.QuantityToAdd > 0)
                {

                    //Hämtar produkten som motsvarar Id:t i ProductsToAddFromInput-listan.
                    var choosenProductToAdd = allProductsFromDb.FirstOrDefault(p => p.Id == inputObject.ProductId);

                    ProductObject.Id = 0;//VIKTIGT med 0 för att databasen ska hantera Id.
                    ProductObject.ArticleNumber = choosenProductToAdd.ArticleNumber;
                    ProductObject.Title = choosenProductToAdd.Title;
                    ProductObject.Description = choosenProductToAdd.Description;
                    ProductObject.Size = choosenProductToAdd.Size;
                    ProductObject.Price = choosenProductToAdd.Price;
                    ProductObject.Category = choosenProductToAdd.Category;
                    ProductObject.Quantity = inputObject.QuantityToAdd;
                    ProductObject.ReceiptId = highestReceiptIdInDb;
                    ProductObject.ShowInAvailableToPurchase = false;
                    ProductObject.ShowInStock = false;
                    ProductObject.ShowOnReceipt = true;
                    await _callApiService.CreateItem("Product", ProductObject); 
                }
            }


            return RedirectToPage("./IncomingProducts");
        }
    }
}
