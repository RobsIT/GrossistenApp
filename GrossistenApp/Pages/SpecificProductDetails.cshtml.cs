using GrossistenApp.Data;
using GrossistenApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrossistenApp.Pages
{
    public class SpecificProductDetailsModel : PageModel
    {
        private readonly GrossistenAppDatabaseContext _context;
        public SpecificProductDetailsModel(GrossistenAppDatabaseContext context)
        {
            _context = context;
        }
        
        public Product SpecificProductDetails { get; set; }
        [BindProperty]
        public Product UpdateSpecificProductDetails { get; set; }
        [BindProperty]
        public Product DeleteSpecificProductObject { get; set; }

        public async Task OnGetAsync(int id)
        {
            
            SpecificProductDetails = await _context.ProductsTable.FirstOrDefaultAsync(p => p.Id == id);
            UpdateSpecificProductDetails = SpecificProductDetails;
            DeleteSpecificProductObject = SpecificProductDetails;
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {

            var productToUpdate = await _context.ProductsTable.FindAsync(UpdateSpecificProductDetails.Id);
            if (productToUpdate == null)
            {
                return NotFound();
            }

            // Uppdatera fälten från det inbundna objektet
            productToUpdate.ArticleNumber = UpdateSpecificProductDetails.ArticleNumber;
            productToUpdate.Title = UpdateSpecificProductDetails.Title;
            productToUpdate.Description = UpdateSpecificProductDetails.Description;
            productToUpdate.Size = UpdateSpecificProductDetails.Size;
            productToUpdate.Price = UpdateSpecificProductDetails.Price;
            productToUpdate.Category = UpdateSpecificProductDetails.Category;
            productToUpdate.Quantity = UpdateSpecificProductDetails.Quantity;

          
            await _context.SaveChangesAsync();
            // Påminner om vilken SpecificProductDetails man är på efter uppdateringen  new { id = UpdateSpecificProductDetails.Id }
            return RedirectToPage("./SpecificProductDetails", new { id = UpdateSpecificProductDetails.Id });
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var productToDelete = await _context.ProductsTable.FindAsync(DeleteSpecificProductObject.Id);

            if (productToDelete == null)
                return NotFound();

            _context.ProductsTable.Remove(productToDelete);
            await _context.SaveChangesAsync();

            return RedirectToPage("/StockOverview");
        }
    }
}
