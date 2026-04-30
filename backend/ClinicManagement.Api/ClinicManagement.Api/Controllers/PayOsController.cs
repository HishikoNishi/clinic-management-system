using System.Collections.Concurrent;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using ClinicManagement.Api.Data;
using ClinicManagement.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ClinicManagement.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Cashier,Admin,Staff")]
    public class PayOsController : ControllerBase
    {
        private static readonly ConcurrentDictionary<long, Guid> OrderMap = new();
        private readonly ClinicDbContext _ctx;
        private readonly PayOsOptions _opt;
        private readonly IHttpClientFactory _http;
        private readonly ILogger<PayOsController> _log;

        public PayOsController(ClinicDbContext ctx, IOptions<PayOsOptions> opt, IHttpClientFactory http, ILogger<PayOsController> log)
        {
            _ctx = ctx;
            _opt = opt.Value;
            _http = http;
            _log = log;
        }

        [HttpPost("create")]
        // Create merchant order from internal invoice. orderCode is numeric because PayOS requires number type.
        public async Task<IActionResult> Create([FromBody] CreatePayOsRequest req)
        {
            try
            {
                if (req?.InvoiceId == Guid.Empty) return BadRequest(new { message = "Thiếu InvoiceId" });
                var invoice = await _ctx.Invoices.FirstOrDefaultAsync(i => i.Id == req.InvoiceId);
                if (invoice == null) return NotFound(new { message = "Không tìm thấy hóa đơn" });
                if (invoice.IsPaid) return BadRequest(new { message = "Hóa đơn đã thanh toán" });

                var amount = invoice.BalanceDue > 0 ? invoice.BalanceDue : invoice.Amount;
                if (amount <= 0) return BadRequest(new { message = "Số tiền không hợp lệ" });

                // orderCode phải là số: dùng 12 hex đầu của Guid -> long (ổn định & duy nhất)
                var hex12 = invoice.Id.ToString("N").Substring(0, 12);
                long orderCode = long.Parse(hex12, System.Globalization.NumberStyles.HexNumber);

                var apptCode = await _ctx.Appointments
                    .Where(a => a.Id == invoice.AppointmentId)
                    .Select(a => a.AppointmentCode)
                    .FirstOrDefaultAsync();

                var description = string.IsNullOrWhiteSpace(apptCode)
                    ? $"Thanh toan hoa don {orderCode}"
                    : $"Thanh toan hoa don {apptCode}";

                var body = new Dictionary<string, object>
                {
                    ["orderCode"] = orderCode,
                    ["amount"] = (int)Math.Round(amount, MidpointRounding.AwayFromZero),
                    ["description"] = description,
                    ["returnUrl"] = _opt.ReturnUrl,
                    ["cancelUrl"] = _opt.CancelUrl
                };

                // Chuỗi ký đúng thứ tự alpha: amount, cancelUrl, description, orderCode, returnUrl
                var raw = $"amount={body["amount"]}&cancelUrl={body["cancelUrl"]}&description={body["description"]}&orderCode={body["orderCode"]}&returnUrl={body["returnUrl"]}";
                body["signature"] = SignHmac(raw, _opt.ChecksumKey ?? "");

                var client = _http.CreateClient();
                client.DefaultRequestHeaders.Add("x-client-id", _opt.ClientId);
                client.DefaultRequestHeaders.Add("x-api-key", _opt.ApiKey);

                var apiUrl = string.IsNullOrWhiteSpace(_opt.ApiCreateUrl)
                    ? "https://api-merchant.payos.vn/v2/payment-requests"
                    : _opt.ApiCreateUrl;

                var resp = await client.PostAsync(apiUrl,
                    new StringContent(JsonSerializer.Serialize(body), Encoding.UTF8, "application/json"));

                var respBody = await resp.Content.ReadAsStringAsync();
                _log.LogInformation("PayOS create {Status}: {Body} | rawSig={Raw}", resp.StatusCode, respBody, raw);

                if (!resp.IsSuccessStatusCode)
                    return StatusCode((int)resp.StatusCode, new { message = "PayOS create failed", body = respBody });

                var env = JsonSerializer.Deserialize<PayOsEnvelope>(respBody,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (env?.data == null)
                    return StatusCode(500, new { message = "PayOS response invalid", body = respBody });

                OrderMap[orderCode] = invoice.Id;

                var qr = env.data.qrCodeUrl ?? env.data.qrCode;
                if (string.IsNullOrWhiteSpace(qr) && !string.IsNullOrWhiteSpace(env.data.qrCodeBase64))
                    qr = $"data:image/png;base64,{env.data.qrCodeBase64}";
                if (string.IsNullOrWhiteSpace(qr) && !string.IsNullOrWhiteSpace(env.data.qrData))
                    qr = env.data.qrData;

                return Ok(new
                {
                    orderCode,
                    amount = body["amount"],
                    checkoutUrl = env.data.checkoutUrl,
                    qrCodeUrl = qr
                });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "PayOS create error");
                return StatusCode(500, new { message = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("webhook")]
        [Consumes("application/json", "application/x-www-form-urlencoded")]
        // Webhook is idempotent: if invoice is already paid, return success and do not duplicate payment rows.
        public async Task<IActionResult> Webhook()
        {
            try
            {
                var (dataDict, signature) = await ReadWebhookDataAsync(Request);
                var orderCodeStr = dataDict.GetValueOrDefault("orderCode");
                var amountStr = dataDict.GetValueOrDefault("amount");

                if (string.IsNullOrWhiteSpace(orderCodeStr))
                    return Ok(new { message = "ignored" });

                if (!string.IsNullOrWhiteSpace(signature))
                {
                    var raw = BuildSignatureString(dataDict);
                    var calc = SignHmac(raw, _opt.ChecksumKey ?? "");
                    if (!calc.Equals(signature, StringComparison.OrdinalIgnoreCase))
                        return Ok(new { message = "invalid signature" });
                }

                // Map orderCode về invoice: orderCode là timestamp/number -> tìm invoice gần nhất có cùng orderCode
            Guid invoiceId;
            if (Guid.TryParse(orderCodeStr, out invoiceId)) { /* ít gặp */ }
            else
            {
                var ocNum = long.Parse(orderCodeStr);
                if (OrderMap.TryGetValue(ocNum, out invoiceId)) { /* map nhanh */ }
                else
                {
                    var inv = await _ctx.Invoices
                        .OrderByDescending(i => i.CreatedAt)
                        .Take(50) // giới hạn quét
                        .ToListAsync();
                    invoiceId = inv
                        .FirstOrDefault(i => long.Parse(i.Id.ToString("N").Substring(0, 12), System.Globalization.NumberStyles.HexNumber) == ocNum)?
                        .Id ?? Guid.Empty;
                    if (invoiceId == Guid.Empty) return Ok(new { message = "ignored" });
                }
            }

                var invoice = await _ctx.Invoices.FirstOrDefaultAsync(i => i.Id == invoiceId);
                if (invoice == null) return Ok(new { message = "ignored" });
                if (invoice.IsPaid) return Ok(new { message = "already paid" });

                var expected = (int)Math.Round(invoice.BalanceDue > 0 ? invoice.BalanceDue : invoice.Amount, MidpointRounding.AwayFromZero);
                if (int.TryParse(amountStr, out var amt) && amt != expected)
                    return Ok(new { message = "amount mismatch" });

                invoice.IsPaid = true;
                invoice.PaymentDate = DateTime.UtcNow;
                _ctx.Payments.Add(new Payment
                {
                    Id = Guid.NewGuid(),
                    InvoiceId = invoice.Id,
                    AppointmentId = invoice.AppointmentId,
                    Amount = expected,
                    Method = PaymentMethod.payosQr,
                    PaymentDate = invoice.PaymentDate.Value
                });
                await _ctx.SaveChangesAsync();
                return Ok(new { message = "paid" });
            }
            catch (Exception ex)
            {
                _log.LogError(ex, "PayOS webhook error");
                return Ok(new { message = "error", detail = ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpGet("webhook")]
        public IActionResult WebhookCheck() => Ok(new { message = "webhook alive" });

        // Helpers
        private static string SignHmac(string data, string key)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(key));
            return BitConverter.ToString(hmac.ComputeHash(Encoding.UTF8.GetBytes(data))).Replace("-", "").ToLowerInvariant();
        }

        private static async Task<(Dictionary<string, string> data, string? signature)> ReadWebhookDataAsync(HttpRequest req)
        {
            var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            string? sig = null;

            if (req.HasFormContentType)
            {
                var form = await req.ReadFormAsync();
                foreach (var kv in form) dict[kv.Key] = kv.Value.ToString();
                form.TryGetValue("signature", out var s); sig = s.ToString();
            }
            else
            {
                using var reader = new StreamReader(req.Body);
                var raw = await reader.ReadToEndAsync();
                if (!string.IsNullOrWhiteSpace(raw))
                {
                    using var doc = JsonDocument.Parse(raw);
                    var el = doc.RootElement;
                    if (el.TryGetProperty("data", out var d)) el = d;
                    foreach (var p in el.EnumerateObject())
                        dict[p.Name] = p.Value.ToString();
                    el.TryGetProperty("signature", out var sigEl); sig = sigEl.ToString();
                }
            }
            return (dict, sig);
        }

        private static string BuildSignatureString(Dictionary<string, string> data)
        {
            var keys = new[] { "amount", "cancelUrl", "description", "orderCode", "returnUrl" };
            return string.Join("&", keys.Where(k => data.ContainsKey(k)).Select(k => $"{k}={data[k]}"));
        }
    }

    public class CreatePayOsRequest { public Guid InvoiceId { get; set; } }
    public class PayOsEnvelope { public PayOsCreateResponse? data { get; set; } }
    public class PayOsCreateResponse
    {
        public string checkoutUrl { get; set; } = string.Empty;
        public string qrCode { get; set; } = string.Empty;
        public string qrCodeUrl { get; set; } = string.Empty;
        public string qrCodeBase64 { get; set; } = string.Empty;
        public string qrData { get; set; } = string.Empty;
        public long orderCode { get; set; }
        public int amount { get; set; }
    }
}
