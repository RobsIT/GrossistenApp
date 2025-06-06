using GrossistenApp.Models;
using GrossistenApp.Service;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrossistenApp.Pages
{
    public class StockOverviewModel : PageModel
    {
        private readonly ICallApiService _callApiService;
        public StockOverviewModel(ICallApiService callApiService)
        {
            _callApiService = callApiService;
        }
       
        [BindProperty]
        public Product ProductObject { get; set; }
        public List<Product> ProductsFromDbList { get; set; } = new(); // null-safe
        public List<Product> StockOverviwProductsFromDbList { get; set; }
        public async Task OnGetAsync()
        {
            List<Product> allProductsFromDbList;

            try
            {
                allProductsFromDbList = await _callApiService.GetDataFromApi<List<Product>>("Product");
            }
            catch (Exception)
            {
                allProductsFromDbList = new List<Product>
                {
                    new Product { Title = "Kunde inte hämta information, testa igen senare(Starta Api).", ShowInStock = true}
                };
            }

            StockOverviwProductsFromDbList = allProductsFromDbList.Where(p => p.ShowInStock ?? false).OrderByDescending(p => p.Id).ToList();

        }
    }
}

