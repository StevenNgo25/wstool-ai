using Microsoft.EntityFrameworkCore;
using System;
using WhatsAppCampaignManager.Data;
using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.HostedServices
{
    public class AppInstanceHealthCheckJob : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<AppInstanceHealthCheckJob> _logger;

        public AppInstanceHealthCheckJob(IServiceProvider serviceProvider, ILogger<AppInstanceHealthCheckJob> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("AppInstanceHealthCheckJob started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();

                    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                    var whapiService = scope.ServiceProvider.GetRequiredService<IWhapiService>();

                    var instances = await dbContext.AppInstances
                        .Where(x => x.IsActive)
                        .ToListAsync(stoppingToken);

                    foreach (var instance in instances)
                    {
                        try
                        {
                            var status = await whapiService.HealthCheck(instance.WhapiToken, instance.WhapiUrl);

                            if (status != null && status != instance.Status)
                            {
                                instance.Status = status;
                                instance.UpdatedAt = DateTime.Now;
                                _logger.LogInformation($"Updated instance {instance.Id} status to {status}");
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, $"Failed to check status for instance {instance.Id}");
                        }
                    }

                    await dbContext.SaveChangesAsync(stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in AppInstanceHealthCheckJob");
                }

                // Wait 5 seconds before next check
                await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
            }

            _logger.LogInformation("AppInstanceHealthCheckJob stopped.");
        }
    }

}
