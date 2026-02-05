namespace CourseWork.Services
{
    public interface IFileService
    {
        Task<string> SaveProductImageAsync(IFormFile file);
        void DeleteFile(string filePath);
    }
}