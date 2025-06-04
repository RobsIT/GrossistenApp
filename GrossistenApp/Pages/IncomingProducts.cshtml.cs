using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrossistenApp.Models;
using GrossistenApp.ViewModels;
using GrossistenApp.Service;

namespace GrossistenApp.Pages
{
    public class IncomingProductsModel : PageModel
    {
        private readonly ICallApiService _callApiService;
        public IncomingProductsModel(ICallApiService callApiService)
        {
            _callApiService = callApiService;
        }
        
        [BindProperty]
        public Product ProductObject { get; set; }
       
        public List<Product> IncomingProductsFromDbList { get; set; }

        public List<Product> ProductsFromDbListOnReceipt{ get; set; }
       
        [BindProperty]
        public List<ProductInputViewModel> ProductsToAddFromInput { get; set; }
       
        public List<Receipt> IncomingReceiptsFromDbList { get; set; }
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
                    new Product { Title = "Kunde inte hämta information, testa igen senare(Starta Api).", ShowInAvailableToPurchase = true}
                };
            }
            IncomingProductsFromDbList = allProductsFromDbList.Where(p => p.ShowInAvailableToPurchase ?? false).OrderByDescending(p => p.Id).ToList();
            ProductsFromDbListOnReceipt = allProductsFromDbList.Where(p => p.ShowOnReceipt ?? false).ToList();


            List<Receipt> allReceiptsFromDbList;

            try
            {
                allReceiptsFromDbList = await _callApiService.GetDataFromApi<List<Receipt>>("Receipt");
            }
            catch (Exception)
            {
                allReceiptsFromDbList = new List<Receipt>
                {
                    new Receipt { WorkerName = "Kunde inte hämta information", showAsIncomingReceipt = true }
                };
            }
            IncomingReceiptsFromDbList = allReceiptsFromDbList.Where(r => r.showAsIncomingReceipt ?? false).OrderByDescending(r => r.DateAndTimeCreated).ToList();
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

            //Increase quantity from the Orderable Products form for the choosen products
            foreach (var inputObject in ProductsToAddFromInput)
            {
                if (inputObject.QuantityToAdd > 0)
                {
                    var specificProduct = allProductsFromDb.FirstOrDefault(p => p.Id == inputObject.ProductId);
                    if (specificProduct != null)
                    {
                        
                        specificProduct.Quantity = specificProduct.Quantity + inputObject.QuantityToAdd;

                        //Update the showInStock boolean if stock is greater than 0. SQL DB reads true/false as 1/0
                        specificProduct.ShowInStock = specificProduct.Quantity > 0;
                    }
                
                }
            }

           await _callApiService.EditItem("Product/bulk", allProductsFromDb);

            //-------Create Receipt---------------   
            ReceiptObject.WorkerName = "Lasse";
            ReceiptObject.showAsIncomingReceipt = true;
            ReceiptObject.showAsOutgoingReceipt = false;
            ReceiptObject.DateAndTimeCreated = DateTime.Now;

            await _callApiService.CreateItem("Receipt", ReceiptObject);

            //Add the increased products to the receipt
            var receipts = await _callApiService.GetDataFromApi<List<Receipt>>("Receipt");
            int highestReceiptIdInDb = receipts.Max(r => r.Id);

            foreach (var inputObject in ProductsToAddFromInput)
            {

                if (inputObject.QuantityToAdd > 0)
                {

                    var choosenProductToAdd = allProductsFromDb.FirstOrDefault(p => p.Id == inputObject.ProductId);

                    ProductObject.Id = 0;//IMPORTANT to use 0 for the database to handle the ID correctly
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
