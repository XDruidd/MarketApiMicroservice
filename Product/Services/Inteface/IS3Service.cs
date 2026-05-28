namespace Product.Services.Inteface;

public interface IS3Service
{
    Task<string> UploadFileAsync(IFormFile file);
    Task UpdateFileAsync(string key, IFormFile file);
    Task<bool> DeleteFileAsync(string key);
}