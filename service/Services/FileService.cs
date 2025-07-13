namespace WhatsAppCampaignManager.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly ILogger<FileService> _logger;

        public FileService(IWebHostEnvironment environment, ILogger<FileService> logger)
        {
            _environment = environment;
            _logger = logger;
        }

        public async Task<string?> SaveFileAsync(IFormFile file, string folder = "images")
        {
            try
            {
                if (file == null || file.Length == 0)
                    return null;

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
                
                if (!allowedExtensions.Contains(fileExtension))
                {
                    _logger.LogWarning("Invalid file type: {Extension}", fileExtension);
                    return null;
                }

                // Validate file size (max 10MB)
                if (file.Length > 10 * 1024 * 1024)
                {
                    _logger.LogWarning("File too large: {Size} bytes", file.Length);
                    return null;
                }

                // Create folder if not exists
                var folderPath = Path.Combine(_environment.WebRootPath, folder);
                if (!Directory.Exists(folderPath))
                {
                    Directory.CreateDirectory(folderPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(folderPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Return relative path
                var relativePath = $"/{folder}/{fileName}";
                _logger.LogInformation("File saved: {FilePath}", relativePath);
                
                return relativePath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving file");
                return null;
            }
        }

        public async Task<bool> DeleteFileAsync(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return false;

                var fullPath = GetFullPath(filePath);
                
                if (File.Exists(fullPath))
                {
                    await Task.Run(() => File.Delete(fullPath));
                    _logger.LogInformation("File deleted: {FilePath}", filePath);
                    return true;
                }

                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting file: {FilePath}", filePath);
                return false;
            }
        }

        public async Task<string?> ConvertFileToBase64Async(string filePath)
        {
            try
            {
                if (string.IsNullOrEmpty(filePath))
                    return null;

                var fullPath = GetFullPath(filePath);
                
                if (!File.Exists(fullPath))
                {
                    _logger.LogWarning("File not found: {FilePath}", filePath);
                    return null;
                }

                var bytes = await File.ReadAllBytesAsync(fullPath);
                var mimeType = GetMimeType(filePath);
                var base64 = Convert.ToBase64String(bytes);
                
                return $"data:{mimeType};base64,{base64}";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error converting file to base64: {FilePath}", filePath);
                return null;
            }
        }

        public async Task<List<string>> GetOldFilesAsync(string folder, TimeSpan olderThan)
        {
            try
            {
                var folderPath = Path.Combine(_environment.WebRootPath, folder);
                
                if (!Directory.Exists(folderPath))
                    return new List<string>();

                var cutoffDate = DateTime.UtcNow - olderThan;
                var oldFiles = new List<string>();

                await Task.Run(() =>
                {
                    var files = Directory.GetFiles(folderPath);
                    foreach (var file in files)
                    {
                        var fileInfo = new FileInfo(file);
                        if (fileInfo.CreationTimeUtc < cutoffDate)
                        {
                            var relativePath = $"/{folder}/{Path.GetFileName(file)}";
                            oldFiles.Add(relativePath);
                        }
                    }
                });

                return oldFiles;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting old files from folder: {Folder}", folder);
                return new List<string>();
            }
        }

        public bool FileExists(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
                return false;

            var fullPath = GetFullPath(filePath);
            return File.Exists(fullPath);
        }

        public string GetFullPath(string relativePath)
        {
            if (string.IsNullOrEmpty(relativePath))
                return string.Empty;

            // Remove leading slash if present
            var cleanPath = relativePath.TrimStart('/');
            return Path.Combine(_environment.WebRootPath, cleanPath);
        }

        private string GetMimeType(string filePath)
        {
            var extension = Path.GetExtension(filePath).ToLowerInvariant();
            return extension switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".bmp" => "image/bmp",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }
    }
}
