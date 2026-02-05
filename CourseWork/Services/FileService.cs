using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

namespace CourseWork.Services
{
    public class FileService : IFileService
    {
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const string DefaultImage = "/images/no-image.png";

        public FileService(IWebHostEnvironment webHostEnvironment)
        {
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<string> SaveProductImageAsync(IFormFile imageFile)
        {
            if (imageFile == null || imageFile.Length == 0) return DefaultImage;

            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(imageFile.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(extension) || imageFile.Length > 5 * 1024 * 1024)
            {
                return DefaultImage;
            }

            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "products");
            if (!Directory.Exists(uploadsFolder)) Directory.CreateDirectory(uploadsFolder);

            string uniqueFileName = Guid.NewGuid().ToString() + extension;
            string filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/products/" + uniqueFileName;
        }

        public void DeleteFile(string imageUrl)
        {
            if (string.IsNullOrEmpty(imageUrl) || imageUrl == DefaultImage) return;

            try
            {
                string relativePath = imageUrl.TrimStart('/');
                string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath);

                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Помилка видалення файлу: {ex.Message}");
            }
        }
    }
}