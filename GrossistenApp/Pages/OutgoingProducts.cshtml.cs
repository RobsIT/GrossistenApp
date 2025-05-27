using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using GrossistenApp.Models;
using Microsoft.EntityFrameworkCore;
using GrossistenApp.Data;
using GrossistenApp.ViewModels;

namespace GrossistenApp.Pages
{
    public class OutgoingProductsModel : PageModel
    {
        private readonly GrossistenAppDatabaseContext _context;
        public OutgoingProductsModel(GrossistenAppDatabaseContext context)
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

        public async Task<IActionResult> OnPostTakeFromStockAsync()
        {

            var allProductsFromDb = await _context.ProductsTable.ToListAsync();

            //----Öka antal från Leveransbara Produkter formuläret på befintlig produkt------

            foreach (var inputObject in ProductsToAddFromInput)
            {
                if (inputObject.QuantityToAdd > 0)
                {
                    var specificProduct = allProductsFromDb.FirstOrDefault(p => p.Id == inputObject.ProductId);
                    if (specificProduct != null)
                    {
                        // ?? 0 betyder att om product.Quantity är null, så används värdet 0 istället.
                        //Den returnerar vänstra sidan om den inte är null, annars returnerar den högra sidan.
                        specificProduct.Quantity = (specificProduct.Quantity ?? 0) - inputObject.QuantityToAdd;

                    }

                }
            }

            await _context.SaveChangesAsync();

            //-------Create Receipt---------------
            ReceiptObject.WorkerName = "Svenne";
            ReceiptObject.showAsIncomingReceipt = false;
            ReceiptObject.showAsOutgoingReceipt = true;
            ReceiptObject.DateAndTimeCreated = DateTime.Now;
            _context.ReceiptsTable.Add(ReceiptObject);

            await _context.SaveChangesAsync();

            //Lägg till dom ökade Produkterna till kvittot
            int highestReceiptIdInDb = _context.ReceiptsTable.Max(x => x.Id);
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
                    _context.ProductsTable.Add(ProductObject);
                    await _context.SaveChangesAsync();
                }
            }

            return RedirectToPage("./OutgoingProducts");
        }
    }
}
