using System.Text.Json;
using ClinicManagement.Api.Models;

namespace ClinicManagement.Api.Services
{
    public class FakeInsuranceService
    {
        private readonly ILogger<FakeInsuranceService> _logger;
        private readonly string _jsonPath;
        private List<InsurancePlan> _plans = new();

        public FakeInsuranceService(IWebHostEnvironment env, ILogger<FakeInsuranceService> logger)
        {
            _logger = logger;
            _jsonPath = Path.Combine(env.ContentRootPath, "insurance.json");
            Load();
        }

        private void Load()
        {
            try
            {
                if (!File.Exists(_jsonPath))
                {
                    _logger.LogWarning("insurance.json not found at {Path}", _jsonPath);
                    return;
                }

                var json = File.ReadAllText(_jsonPath);
                var data = JsonSerializer.Deserialize<List<InsurancePlan>>(json, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                _plans = data ?? new List<InsurancePlan>();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to load insurance plans");
                _plans = new List<InsurancePlan>();
            }
        }

        public InsurancePlan? Verify(string code)
        {
            if (string.IsNullOrWhiteSpace(code)) return null;

            var plan = _plans.FirstOrDefault(p =>
                string.Equals(p.Code, code, StringComparison.OrdinalIgnoreCase));

            if (plan == null) return null;
            if (!plan.IsActive) return null;
            if (plan.ExpiryDate.HasValue && plan.ExpiryDate.Value <= DateTime.UtcNow) return null;

            return plan;
        }
    }
}
