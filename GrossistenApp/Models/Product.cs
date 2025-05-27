

namespace GrossistenApp.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string? ArticleNumber {  get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Size { get; set; }
        public double? Price { get; set; }
        public string? Category { get; set; }
        public int? Quantity {  get; set; }        
        public bool? ShowInAvailableToPurchase { get; set; }
        public bool? ShowInStock { get; set; }       
        public bool? ShowOnReceipt { get; set; }
        public int? ReceiptId { get; set; }
    }

}
