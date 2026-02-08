using PhotoApp.Models;

namespace PhotoApp.Services;

public class PhotoFileService
{
    private readonly string _photosDirectory;

    public PhotoFileService()
    {
        _photosDirectory = "/photos";
    }

    public List<PhotoDto> GetAllPhotos()
    {
        if (!Directory.Exists(_photosDirectory))
        {
            return new List<PhotoDto>();
        }

        var imageExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".bmp", ".tiff" };
        
        var photos = Directory.GetFiles(_photosDirectory, "*.*", SearchOption.TopDirectoryOnly)
            .Where(file => imageExtensions.Contains(Path.GetExtension(file).ToLower()))
            .Select(file => new FileInfo(file))
            .Select(fileInfo => new PhotoDto(
                fileInfo.Name,
                $"/photos/{fileInfo.Name}",
                fileInfo.Length,
                fileInfo.LastWriteTimeUtc
            ))
            .OrderBy(photo => photo.DateModified)
            .ToList();

        return photos;
    }

    public PhotoDto? GetPhotoByFileName(string fileName)
    {
        var filePath = Path.Combine(_photosDirectory, fileName);
        if (!File.Exists(filePath))
        {
            return null;
        }

        var fileInfo = new FileInfo(filePath);
        var contentType = GetContentType(filePath);
        
        return new PhotoDto(
            fileInfo.Name,
            $"/photos/{fileInfo.Name}",
            fileInfo.Length,
            fileInfo.LastWriteTimeUtc
        );
    }

    private string GetContentType(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLower();
        return extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".webp" => "image/webp",
            ".bmp" => "image/bmp",
            ".tiff" => "image/tiff",
            _ => "application/octet-stream"
        };
    }

    public List<PhotoDto> GetPhotosByPaths(string[] photoPaths)
    {
        return photoPaths
            .Select(path => GetPhotoByFileName(Path.GetFileName(path)))
            .Where(photo => photo != null)
            .Cast<PhotoDto>()
            .OrderBy(photo => photo.DateModified)
            .ToList();
    }
}
