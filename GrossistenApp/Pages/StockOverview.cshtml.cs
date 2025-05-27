using GrossistenApp.Data;
using GrossistenApp.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace GrossistenApp.Pages
{
    public class StockOverviewModel : PageModel
    {
        private readonly GrossistenAppDatabaseContext _context;
        public StockOverviewModel(GrossistenAppDatabaseContext context)
        {
            _context = context;
        }
        [BindProperty]
        public Product ProductObject { get; set; }
        //Lägg till ProductsFromDbList som null-säkert i modellen " = new() "
        public List<Product> ProductsFromDbList { get; set; } = new(); // null-safe

        public async Task OnGetAsync()
        {
            ProductsFromDbList = await _context.ProductsTable
                .Where(p => p.ShowInStock == true)
                .ToListAsync();
        }
    }
}

