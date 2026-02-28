using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using PhotoScavengerHunt.Services.Interfaces;

namespace PhotoScavengerHunt.Services
{
    public class CloudinaryStorageService : IStorageService
    {
        private readonly Cloudinary _cloudinary;
        private readonly ILogger<CloudinaryStorageService> _logger;

        public CloudinaryStorageService(IConfiguration config, ILogger<CloudinaryStorageService> logger)
        {
            _logger = logger;
            var name = config["Cloudinary:CloudName"];
            var key = config["Cloudinary:ApiKey"];
            var secret = config["Cloudinary:ApiSecret"];
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(key) || string.IsNullOrWhiteSpace(secret))
                throw new ArgumentException("Cloudinary configuration missing");

            var account = new Account(name, key, secret);
            _cloudinary = new Cloudinary(account) { Api = { Secure = true } };
        }

        public async Task<string> UploadFileAsync(IFormFile file, string? folder = null)
        {
            if (file == null) throw new ArgumentNullException(nameof(file));

            var uploadParams = new ImageUploadParams
            {
                File = new FileDescription(file.FileName, file.OpenReadStream()),
                Folder = folder,
                UseFilename = true,
                UniqueFilename = true,
                Overwrite = false
            };

            var res = await _cloudinary.UploadAsync(uploadParams);
            if (res.StatusCode != System.Net.HttpStatusCode.OK && res.StatusCode != System.Net.HttpStatusCode.Created)
            {
                _logger.LogError("Cloudinary upload failed: {Status} {Error}", res.StatusCode, res.Error?.Message);
                throw new InvalidOperationException(res.Error?.Message ?? "Cloudinary upload failed");
            }

            return res.SecureUrl?.ToString() ?? res.Url?.ToString() ?? throw new InvalidOperationException("No URL returned from Cloudinary");
        }

        public async Task DeleteFileAsync(string fileUrl)
        {
            if (string.IsNullOrWhiteSpace(fileUrl)) return;
            try
            {
                var publicId = fileUrl.Split('/').LastOrDefault()?.Split('.').FirstOrDefault();
                if (string.IsNullOrWhiteSpace(publicId)) return;
                var delParams = new DeletionParams(publicId) { ResourceType = ResourceType.Image };
                await _cloudinary.DestroyAsync(delParams);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Cloudinary delete failed for {Url}", fileUrl);
            }
        }
    }
}