

using System.ComponentModel.DataAnnotations;

namespace GrossistenApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "(Model)Artikelnummer är obligatoriskt.")]
        [MaxLength(20, ErrorMessage = "(Model)Artikelnummer får vara max 20 tecken.")]
        [RegularExpression(@"^[a-zA-Z0-9]+$", ErrorMessage = "(Model)Artikelnummer får endast innehålla bokstäver och siffror.")]
        public string? ArticleNumber {  get; set; }
        
        [Required(ErrorMessage = "(Model)Titel är obligatoriskt.")]
        [MaxLength(20, ErrorMessage = "(Model)Titel får vara max 20 tecken.")]
        public string? Title { get; set; }

        [Required(ErrorMessage = "(Model)Beskrivning är obligatoriskt.")]
        [MaxLength(30, ErrorMessage = "(Model)Beskrivning får vara max 100 tecken.")]
        public string? Description { get; set; }

        [Required(ErrorMessage = "(Model)Storlek är obligatoriskt.")]
        [MaxLength(20, ErrorMessage = "(Model)Storlek får vara max 20 tecken.")]
        public string? Size { get; set; }

        [Required(ErrorMessage = "(Model)Pris är obligatoriskt.")]
        [Range(0, 100000, ErrorMessage = "(Model)Pris måste vara mellan 0 och 100000.")]
        public double? Price { get; set; }

        [Required(ErrorMessage = "(Model)Kategori är obligatoriskt.")]
        [MaxLength(20, ErrorMessage = "(Model)Kategori får vara max 20 tecken.")]
        [RegularExpression(@"^[a-zA-ZåäöÅÄÖ\s]+$", ErrorMessage = "(Model)Kategori får endast innehålla bokstäver.")]
        public string? Category { get; set; }

        [Range(0, 10000, ErrorMessage = "(Model)Antal måste vara mellan 0 och 10000.")]
        public int? Quantity {  get; set; }        
        public bool? ShowInAvailableToPurchase { get; set; }
        public bool? ShowInStock { get; set; }       
        public bool? ShowOnReceipt { get; set; }
        public int? ReceiptId { get; set; }
    }

}
