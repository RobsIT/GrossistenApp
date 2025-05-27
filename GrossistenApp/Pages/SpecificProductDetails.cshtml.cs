
using GrossistenApp.Models;
using GrossistenApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;


namespace GrossistenApp.Pages
{
    public class SpecificProductDetailsModel : PageModel
    {
        private readonly CallApiService _callApiService;
        public SpecificProductDetailsModel(CallApiService callApiService)
        {
            _callApiService = callApiService;
        }
        
        public Product SpecificProductDetails { get; set; }
        [BindProperty]
        public Product UpdateSpecificProductDetails { get; set; }
        [BindProperty]
        public Product DeleteSpecificProductObject { get; set; }

        public async Task OnGetAsync(int id)
        {
            
            SpecificProductDetails = await _callApiService.GetDataFromApi<Product>($"Product/{id}");    
            UpdateSpecificProductDetails = SpecificProductDetails;
            DeleteSpecificProductObject = SpecificProductDetails;
        }

        public async Task<IActionResult> OnPostUpdateAsync()
        {

            var productToUpdate = await _callApiService.GetDataFromApi<Product>($"Product/{UpdateSpecificProductDetails.Id}");
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


            await _callApiService.EditItem($"Product/{productToUpdate.Id}", productToUpdate);
            // Påminner om vilken SpecificProductDetails man är på efter uppdateringen  new { id = UpdateSpecificProductDetails.Id }
            return RedirectToPage("./SpecificProductDetails", new { id = UpdateSpecificProductDetails.Id });
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var productToDelete = await _callApiService.GetDataFromApi<Product>($"Product/{DeleteSpecificProductObject.Id}");

            if (productToDelete == null)
                return NotFound();

            await _callApiService.DeleteItem($"Product/{DeleteSpecificProductObject.Id}");

            return RedirectToPage("/StockOverview");
        }
    }
}
