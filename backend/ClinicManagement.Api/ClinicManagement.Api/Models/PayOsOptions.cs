namespace ClinicManagement.Api.Models
{
    public class PayOsOptions
    {
        public string ClientId { get; set; } = string.Empty;
        public string ApiKey { get; set; } = string.Empty;
        public string ChecksumKey { get; set; } = string.Empty;
        public string ReturnUrl { get; set; } = string.Empty;
        public string CancelUrl { get; set; } = string.Empty;
        public string WebhookUrl { get; set; } = string.Empty;
        public string SandboxCheckoutBase { get; set; } = "https://sandbox.payos.vn/checkout";
        public string CheckoutBase { get; set; } = "https://pay.payos.vn/checkout";
        public string ApiCreateUrl { get; set; } = "https://api-merchant.payos.vn/v2/payment-requests";
        // Thông tin ngân hàng (để render QR VietQR hiển thị tên TK, mô tả)
        public string BankBin { get; set; } = "970422"; // MB mặc định
        public string BankNumber { get; set; } = string.Empty;
        public string AccountName { get; set; } = string.Empty;
    }
}
