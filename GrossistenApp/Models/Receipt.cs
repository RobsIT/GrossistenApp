namespace GrossistenApp.Models
{
    public class Receipt   
    {
        public int Id { get; set; }
        public string? WorkerName { get; set; }
        public DateTime? DateAndTimeCreated {  get; set; }
        public bool? showAsIncomingReceipt {  get; set; }
        public bool? showAsOutgoingReceipt {  get; set; }
    
    }
}
