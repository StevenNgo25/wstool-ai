using WhatsAppCampaignManager.Services;

namespace WhatsAppCampaignManager.HostedServices
{
    public class FileCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<FileCleanupService> _logger;
        private readonly TimeSpan _cleanupInterval = TimeSpan.FromDays(1); // Check daily
        private readonly TimeSpan _fileMaxAge = TimeSpan.FromDays(14); // Delete files older than 2 weeks

        public FileCleanupService(IServiceProvider serviceProvider, ILogger<FileCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("File Cleanup Service started - will delete files older than {Days} days", _fileMaxAge.TotalDays);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldFiles(stoppingToken);
                    await Task.Delay(_cleanupInterval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("File Cleanup Service is stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in File Cleanup Service");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken); // Wait 1 hour before retry
                }
            }
        }

        private async Task CleanupOldFiles(CancellationToken stoppingToken)
        {
            using var scope = _serviceProvider.CreateScope();
            var fileService = scope.ServiceProvider.GetRequiredService<IFileService>();

            try
            {
                _logger.LogInformation("Starting file cleanup process");

                // Get old files from images folder
                var oldFiles = await fileService.GetOldFilesAsync("images", _fileMaxAge);

                if (!oldFiles.Any())
                {
                    _logger.LogInformation("No old files found to cleanup");
                    return;
                }

                _logger.LogInformation("Found {Count} old files to cleanup", oldFiles.Count);

                var deletedCount = 0;
                foreach (var filePath in oldFiles)
                {
                    if (stoppingToken.IsCancellationRequested)
                        break;

                    try
                    {
                        var deleted = await fileService.DeleteFileAsync(filePath);
                        if (deleted)
                        {
                            deletedCount++;
                            _logger.LogDebug("Deleted old file: {FilePath}", filePath);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete file: {FilePath}", filePath);
                    }
                }

                _logger.LogInformation("File cleanup completed. Deleted {DeletedCount} out of {TotalCount} files", 
                    deletedCount, oldFiles.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during file cleanup process");
            }
        }
    }
}
