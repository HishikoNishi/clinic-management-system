using ClinicManagement.Api.Services;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

namespace ClinicManagement.Api.Services 
{
    public class NoShowBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;

        public NoShowBackgroundService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _serviceProvider.CreateScope();

                var service = scope.ServiceProvider
                    .GetRequiredService<AppointmentService>();

                await service.MarkNoShowAppointments();

                await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);
            }
        }
    }
}