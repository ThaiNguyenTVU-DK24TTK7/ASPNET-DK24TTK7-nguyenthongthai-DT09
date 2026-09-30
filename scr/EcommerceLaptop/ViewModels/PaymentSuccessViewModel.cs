namespace Ecommerce.ViewModels
{
    public class PaymentSuccessViewModel
    {
        public string OrderId { get; set; } = "";
        public string Amount { get; set; } = "";
        public string TransactionId { get; set; } = "";
        public string PaymentMethod { get; set; } = "";
    }
}