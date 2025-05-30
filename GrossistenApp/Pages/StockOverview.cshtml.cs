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
        //Lägg till ProductsFromDbList som null-säkert i modellen " = new() "
        public List<Product> StockOverviwProductsFromDbList{ get; set; } = new(); // null-safe

        public async Task OnGetAsync()
        {
            var AllProductsFromDbList = await _callApiService.GetDataFromApi<List<Product>>("Product");
            StockOverviwProductsFromDbList = AllProductsFromDbList.Where(p => p.ShowInStock ?? false).OrderByDescending(p => p.Id).ToList();

        }
    }
}

