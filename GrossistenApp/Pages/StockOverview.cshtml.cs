using GrossistenApp.Models;
using GrossistenApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrossistenApp.Pages
{
    public class StockOverviewModel : PageModel
    {
        private readonly CallApiService _callApiService;
        public StockOverviewModel(CallApiService callApiService)
        {
            _callApiService = callApiService;
        }
        [BindProperty]
        public Product ProductObject { get; set; }
        //L�gg till ProductsFromDbList som null-s�kert i modellen " = new() "
        public List<Product> ProductsFromDbList { get; set; } = new(); // null-safe

        public async Task OnGetAsync()
        {
            ProductsFromDbList = await _callApiService.GetDataFromApi<List<Product>>("Product");
        }
    }
}

