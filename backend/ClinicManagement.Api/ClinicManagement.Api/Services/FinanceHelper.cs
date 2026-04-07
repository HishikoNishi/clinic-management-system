namespace ClinicManagement.Api.Services
{
    public static class FinanceHelper
    {
        public static decimal Clamp01(decimal value)
        {
            if (value < 0) return 0m;
            if (value > 1) return 1m;
            return value;
        }
    }
}
