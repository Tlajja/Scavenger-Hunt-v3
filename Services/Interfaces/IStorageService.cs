using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace PhotoScavengerHunt.Services.Interfaces
{
    public interface IStorageService
    {
        // Uploads file and returns a publicly accessible URL
        Task<string> UploadFileAsync(IFormFile file, string? folder = null);

        // Optionally delete by URL
        Task DeleteFileAsync(string fileUrl);
    }
}