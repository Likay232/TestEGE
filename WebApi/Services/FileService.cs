namespace WebApi.Services;

public class FileService(IWebHostEnvironment env)
{
    public async Task<bool> SaveFileToRepo(IFormFile file, string fileName)
    {
        var path = Path.Combine(env.ContentRootPath, "FileRepository", fileName);

        await using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream);
        
        return true;
    }

    public async Task<byte[]?> GetFileBytes(string fileName)
    {
        var filePath = Path.Combine(env.ContentRootPath, "FileRepository", fileName);
        
        if (!File.Exists(filePath))
            return null;

        var fileBytes = await File.ReadAllBytesAsync(filePath);
        
        return fileBytes;
    }

}