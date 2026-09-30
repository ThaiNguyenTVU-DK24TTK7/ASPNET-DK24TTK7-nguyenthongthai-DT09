using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace Ecommerce.Services
{
    public class MoMoService
    {
        private readonly string _partnerCode;
        private readonly string _accessKey;
        private readonly string _secretKey;
        private readonly string _endpoint;
        private readonly string _returnUrl;
        private readonly string _notifyUrl;

        private readonly IHttpContextAccessor _httpContextAccessor;

        public MoMoService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
        {
            _partnerCode = configuration["MoMo:PartnerCode"] ?? "MOMOBKUN20180529";
            _accessKey = configuration["MoMo:AccessKey"] ?? "klm05TvNBzhg7h7j";
            _secretKey = configuration["MoMo:SecretKey"] ?? "at67qH6mk8w5Y1nAyMoYKMWACiEi2bsa";
            _endpoint = configuration["MoMo:Endpoint"] ?? "https://test-payment.momo.vn/v2/gateway/api/create";
            _httpContextAccessor = httpContextAccessor;
            var request = _httpContextAccessor.HttpContext?.Request;
            if (request != null)
            {
                var baseUrl = $"{request.Scheme}://{request.Host}";
                _returnUrl = $"{baseUrl}/ThanhToan/PaymentReturn";
                _notifyUrl = $"{baseUrl}/ThanhToan/PaymentNotify";
            }
            else
            {
                _returnUrl = configuration["MoMo:ReturnUrl"] ?? "http://localhost:5192/ThanhToan/PaymentReturn";
                _notifyUrl = configuration["MoMo:NotifyUrl"] ?? "http://localhost:5192/ThanhToan/PaymentNotify";
            }
        }

        public async Task<MoMoCreatePaymentResponse> CreatePaymentAsync(MoMoCreatePaymentRequest request)
        {
            try
            {
                var requestId = Guid.NewGuid().ToString();
                var orderId = $"DH{request.OrderId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
                var orderInfo = $"Thanh toán đơn hàng #{request.OrderId} - {request.CustomerName}";
                var amount = request.Amount.ToString("0");
                var extraData = "";
                var requestType = "captureWallet";
                var rawSignature = $"accessKey={_accessKey}&amount={amount}&extraData={extraData}&ipnUrl={_notifyUrl}&orderId={orderId}&orderInfo={orderInfo}&partnerCode={_partnerCode}&redirectUrl={_returnUrl}&requestId={requestId}&requestType={requestType}";
                var signature = ComputeHmacSha256(rawSignature, _secretKey);

                var requestData = new
                {
                    partnerCode = _partnerCode,
                    accessKey = _accessKey,
                    requestId = requestId,
                    amount = amount,
                    orderId = orderId,
                    orderInfo = orderInfo,
                    redirectUrl = _returnUrl,
                    ipnUrl = _notifyUrl,
                    extraData = extraData,
                    requestType = requestType,
                    signature = signature,
                    lang = "vi",
                    autoCapture = true,
                    orderGroupId = ""
                };

                using var httpClient = new HttpClient();
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PostAsync(_endpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();
                
                var result = JsonConvert.DeserializeObject<MoMoCreatePaymentResponse>(responseContent);
                
                if (result != null)
                {
                    result.RequestId = requestId;
                    result.OrderId = orderId;
                    result.Success = result.ResultCode == "0";
                }

                return result ?? new MoMoCreatePaymentResponse { Success = false, Message = "Invalid response" };
            }
            catch (Exception ex)
            {
                return new MoMoCreatePaymentResponse 
                { 
                    Success = false, 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        public async Task<MoMoQueryResponse> QueryPaymentAsync(string orderId, string requestId)
        {
            try
            {
                var rawSignature = $"accessKey={_accessKey}&orderId={orderId}&partnerCode={_partnerCode}&requestId={requestId}";
                var signature = ComputeHmacSha256(rawSignature, _secretKey);

                var requestData = new
                {
                    partnerCode = _partnerCode,
                    accessKey = _accessKey,
                    requestId = requestId,
                    orderId = orderId,
                    signature = signature,
                    lang = "vi"
                };

                using var httpClient = new HttpClient();
                var json = JsonConvert.SerializeObject(requestData);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var queryEndpoint = "https://test-payment.momo.vn/v2/gateway/api/query";
                var response = await httpClient.PostAsync(queryEndpoint, content);
                var responseContent = await response.Content.ReadAsStringAsync();

                var result = JsonConvert.DeserializeObject<MoMoQueryResponse>(responseContent);
                return result ?? new MoMoQueryResponse { ResultCode = "1001", Message = "Query failed" };
            }
            catch (Exception ex)
            {
                return new MoMoQueryResponse 
                { 
                    ResultCode = "1001", 
                    Message = $"Error: {ex.Message}" 
                };
            }
        }

        private string ComputeHmacSha256(string message, string secretKey)
        {
            var keyBytes = Encoding.UTF8.GetBytes(secretKey);
            var messageBytes = Encoding.UTF8.GetBytes(message);

            using (var hmac = new HMACSHA256(keyBytes))
            {
                var hashBytes = hmac.ComputeHash(messageBytes);
                return BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
            }
        }
    }

    public class MoMoCreatePaymentRequest
    {
        public int OrderId { get; set; }
        public decimal Amount { get; set; }
        public string CustomerName { get; set; } = "";
    }

    public class MoMoCreatePaymentResponse
    {
        public string RequestId { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string ResultCode { get; set; } = "";
        public string Message { get; set; } = "";
        public string PayUrl { get; set; } = "";
        public string QrCodeUrl { get; set; } = "";
        public string Deeplink { get; set; } = "";
        public string DeeplinkMiniApp { get; set; } = "";
        public bool Success { get; set; }
    }

    public class MoMoQueryResponse
    {
        public string ResultCode { get; set; } = "";
        public string Message { get; set; } = "";
        public string OrderId { get; set; } = "";
        public string Amount { get; set; } = "";
        public string OrderInfo { get; set; } = "";
        public string OrderType { get; set; } = "";
        public string TransId { get; set; } = "";
        public string PayType { get; set; } = "";
    }
}