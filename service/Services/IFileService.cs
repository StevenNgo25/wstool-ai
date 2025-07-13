namespace WhatsAppCampaignManager.Services
{
    public interface IFileService
    {
        Task<string?> SaveFileAsync(IFormFile file, string folder = "images");
        Task<bool> DeleteFileAsync(string filePath);
        Task<string?> ConvertFileToBase64Async(string filePath);
        Task<List<string>> GetOldFilesAsync(string folder, TimeSpan olderThan);
        bool FileExists(string filePath);
        string GetFullPath(string relativePath);
    }
}
