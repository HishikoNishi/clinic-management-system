using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClinicManagement.Api.Services
{
    public interface IPricingProvider
    {
        decimal ConsultationFee { get; }
        decimal ClinicTicketFee { get; }
        decimal DefaultTestPrice { get; }
        decimal GetDrugPrice(string medicineName);
        decimal GetTestPrice(string testName);
    }

    public class PricingProvider : IPricingProvider
    {
        private readonly IConfiguration _configuration;
        private PricingCache _cache = null!;
        private readonly object _lock = new();

        public PricingProvider(IConfiguration configuration)
        {
            _configuration = configuration;
            Load();
        }

        public decimal ConsultationFee => _cache.ConsultationFee;

        public decimal DefaultTestPrice => _cache.DefaultTestPrice;
        public decimal ClinicTicketFee => _cache.ClinicTicketFee;

        public decimal GetDrugPrice(string medicineName)
        {
            if (string.IsNullOrWhiteSpace(medicineName)) return 0m;
            EnsureLoaded();
            return _cache.DrugPrices
                .FirstOrDefault(x => string.Equals(x.Name, medicineName, StringComparison.OrdinalIgnoreCase))
                ?.Price ?? 0m;
        }

        public decimal GetTestPrice(string testName)
        {
            if (string.IsNullOrWhiteSpace(testName)) return 0m;
            EnsureLoaded();
            var price = _cache.TestPrices
                .FirstOrDefault(x => string.Equals(x.Name, testName, StringComparison.OrdinalIgnoreCase))
                ?.Price ?? 0m;
            if (price <= 0) price = _cache.DefaultTestPrice;
            return price;
        }

        private void EnsureLoaded()
        {
            if (_cache != null) return;
            lock (_lock)
            {
                if (_cache == null) Load();
            }
        }

        private void Load()
        {
            var section = _configuration.GetSection("Billing");
            PricingConfig? data = null;

            if (section.Exists() && section.GetChildren().Any())
            {
                data = section.Get<PricingConfig>();
            }

            // fallback: đọc trực tiếp root (billing.json không có section Billing)
            if (data == null || (data.ConsultationFee == null && data.DefaultTestPrice == null
                                 && (data.DrugPrices == null || data.DrugPrices.Count == 0)
                                 && (data.TestPrices == null || data.TestPrices.Count == 0)))
            {
                data = _configuration.Get<PricingConfig>();
            }

            _cache = new PricingCache
            {
                ConsultationFee = data?.ConsultationFee ?? 0m,
                ClinicTicketFee = data?.ClinicTicketFee ?? data?.ConsultationFee ?? 0m,
                DefaultTestPrice = data?.DefaultTestPrice ?? 0m,
                DrugPrices = data?.DrugPrices ?? new List<NamedPrice>(),
                TestPrices = data?.TestPrices ?? new List<NamedPrice>()
            };
        }

        private class PricingCache
        {
            public decimal ConsultationFee { get; set; }
            public decimal ClinicTicketFee { get; set; }
            public decimal DefaultTestPrice { get; set; }
            public List<NamedPrice> DrugPrices { get; set; } = new();
            public List<NamedPrice> TestPrices { get; set; } = new();
        }

        public class PricingConfig
        {
            public decimal? ConsultationFee { get; set; }
            public decimal? ClinicTicketFee { get; set; }
            public decimal? DefaultTestPrice { get; set; }
            public List<NamedPrice> DrugPrices { get; set; } = new();
            public List<NamedPrice> TestPrices { get; set; } = new();
        }
    }

    public class NamedPrice
    {
        public string Name { get; set; } = string.Empty;
        public decimal Price { get; set; }
    }
}
