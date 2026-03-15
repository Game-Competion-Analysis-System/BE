using DAL.Repository;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace BIL.Service
{
    public class CloudinarySyncWorker(IServiceProvider serviceProvider, ILogger<CloudinarySyncWorker> logger) : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        private readonly ILogger<CloudinarySyncWorker> _logger = logger;
        private const int INTERVAL_SECONDS = 60; // 1 minute

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("CloudinarySyncWorker started. Checking for new images every {Interval}s", INTERVAL_SECONDS);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceProvider.CreateScope();
                    var repo = scope.ServiceProvider.GetRequiredService<IAIAnalysisRepository>();

                    // Using a default UserID (e.g. 1). In production, this should be a system account.
                    int systemUserId = 1;

                    _logger.LogDebug("Checking Cloudinary for new screenshots...");
                    var result = await repo.ProcessLatestImageFromCloudAsync(systemUserId);

                    if (result != null)
                    {
                        _logger.LogInformation("Successfully processed new image from Cloudinary. Analysis ID: {AnalysisId}", result.Analysisid);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error occurred in CloudinarySyncWorker");
                }

                await Task.Delay(TimeSpan.FromSeconds(INTERVAL_SECONDS), stoppingToken);
            }
        }
    }
}
