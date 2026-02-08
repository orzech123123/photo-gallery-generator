namespace PhotoApp.Models;

public record PhotoDto(
    string FileName,
    string FilePath,
    long FileSize,
    DateTime DateModified
);

public record PhotoListDto(
    string FileName,
    long FileSize,
    DateTime DateModified
);

public record GenerateAlbumRequest(
    string[] PhotoPaths
);