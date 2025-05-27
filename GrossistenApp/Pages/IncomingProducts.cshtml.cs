using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrossistenApp.Models;
using Microsoft.EntityFrameworkCore;
using GrossistenApp.Data;
using GrossistenApp.ViewModels;

namespace GrossistenApp.Pages
{
    public class IncomingProductsModel : PageModel
    {
        private readonly GrossistenAppDatabaseContext _context;
        public IncomingProductsModel(GrossistenAppDatabaseContext context)
        {
            _context = context;
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
            ProductsFromDbList = await _context.ProductsTable.ToListAsync();
            ReceiptsFromDbList = await _context.ReceiptsTable.ToListAsync();

        }

        //Form 1 in View
        public async Task<IActionResult> OnPostCreateProductAsync()
        {
            ProductObject.Quantity = 0;
            ProductObject.ReceiptId = 0;
            ProductObject.ShowInAvailableToPurchase = true;
            ProductObject.ShowInStock = false;
            ProductObject.ShowOnReceipt = false;
            _context.ProductsTable.Add(ProductObject);
            await _context.SaveChangesAsync();
            return RedirectToPage("./IncomingProducts");
        }

        //Form 2 in View
        public async Task<IActionResult> OnPostAddToStockAsync()
        {

            var allProductsFromDb = await _context.ProductsTable.ToListAsync();

            //----�ka antal fr�n Best�llningsbara Produkter formul�ret p� befintlig produkt------
            foreach (var inputObject in ProductsToAddFromInput)
            {
                if (inputObject.QuantityToAdd > 0)
                {
                    var specificProduct = allProductsFromDb.FirstOrDefault(p => p.Id == inputObject.ProductId);
                    if (specificProduct != null)
                    {
                        
                        specificProduct.Quantity = specificProduct.Quantity + inputObject.QuantityToAdd;

                        // Uppdatera showInStock boolen om lagret �r mer �n antal 0. Sql-Db l�ser true/faulse som 1/0.
                        specificProduct.ShowInStock = specificProduct.Quantity > 0;
                    }
                
                }
            }

           await _context.SaveChangesAsync();

            //-------Create Receipt---------------
            ReceiptObject.WorkerName = "Svenne";
            ReceiptObject.showAsIncomingReceipt = true;
            ReceiptObject.showAsOutgoingReceipt = false;
            ReceiptObject.DateAndTimeCreated = DateTime.Now;
            _context.ReceiptsTable.Add(ReceiptObject);

            await _context.SaveChangesAsync();

            //L�gg till dom �kade Produkterna till kvittot
            int highestReceiptIdInDb = _context.ReceiptsTable.Max(x => x.Id);
            foreach (var inputObject in ProductsToAddFromInput)
            {

                if (inputObject.QuantityToAdd > 0)
                {

                    //H�mtar produkten som motsvarar Id:t i ProductsToAddFromInput-listan.
                    var choosenProductToAdd = allProductsFromDb.FirstOrDefault(p => p.Id == inputObject.ProductId);

                    ProductObject.Id = 0;//VIKTIGT med 0 f�r att databasen ska hantera Id.
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
                    _context.ProductsTable.Add(ProductObject);
                    await _context.SaveChangesAsync();
                }
            }


            return RedirectToPage("./IncomingProducts");
        }
    }
}
