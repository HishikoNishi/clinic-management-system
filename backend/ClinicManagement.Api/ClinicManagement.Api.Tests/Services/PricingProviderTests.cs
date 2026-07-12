using ClinicManagement.Api.Services;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ClinicManagement.Api.Tests.Services;

public class PricingProviderTests
{
    private static PricingProvider CreateSut()
    {
        var values = new Dictionary<string, string?>
        {
            ["Billing:ConsultationFee"] = "100000",
            ["Billing:ClinicTicketFee"] = "120000",
            ["Billing:DefaultTestPrice"] = "80000",
            ["Billing:DrugPrices:0:Name"] = "Paracetamol",
            ["Billing:DrugPrices:0:Price"] = "1500",
            ["Billing:TestPrices:0:Name"] = "X-Ray",
            ["Billing:TestPrices:0:Price"] = "180000"
        };

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(values)
            .Build();

        return new PricingProvider(configuration);
    }

    [Fact]
    public void GetDrugPrice_MatchesCaseInsensitivePrefixAtWordBoundary()
    {
        var sut = CreateSut();

        Assert.Equal(1500m, sut.GetDrugPrice("paracetamol 500mg"));
        Assert.Equal(0m, sut.GetDrugPrice("ParacetamolX"));
    }

    [Fact]
    public void GetTestPrice_ReturnsConfiguredPriceOrDefaultPrice()
    {
        var sut = CreateSut();

        Assert.Equal(180000m, sut.GetTestPrice("x-ray"));
        Assert.Equal(80000m, sut.GetTestPrice("Xét nghiệm khác"));
    }

    [Fact]
    public void Fees_AreLoadedFromBillingConfiguration()
    {
        var sut = CreateSut();

        Assert.Equal(100000m, sut.ConsultationFee);
        Assert.Equal(120000m, sut.ClinicTicketFee);
    }
}
