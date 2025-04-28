using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace FormsApp.Features.Templates.Services
{
    public interface ICloudStorageService
    {
        Task<string> UploadImageAsync(IFormFile imageFile);
        Task DeleteImageAsync(string imageUrl);
        Task<string> GetImageUrlAsync(string imageName);
    }
} 