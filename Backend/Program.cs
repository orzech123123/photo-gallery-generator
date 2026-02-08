using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;
using PhotoApp.Models;
using PhotoApp.Services;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Configure Kestrel to handle large request bodies
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.Limits.MaxRequestBodySize = 2L * 1024 * 1024 * 1024; // 2 GB
});

// Register services
builder.Services.AddSingleton<PdfService>();
builder.Services.AddSingleton<PhotoFileService>();

// Configure JSON serialization to use camelCase
builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
});

// Allow all CORS for local development
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy
            .AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors("AllowAll");
app.UseStaticFiles();

// Serve the main HTML file at root
app.MapGet("/", async () =>
{
    var indexPath = Path.Combine(app.Environment.ContentRootPath, "..", "index.html");
    if (!File.Exists(indexPath))
    {
        return Results.NotFound("index.html not found");
    }
    
    var content = await File.ReadAllTextAsync(indexPath);
    return Results.Text(content, "text/html");
});

// GET /api/photos - Get all photos (list without base64 data)
app.MapGet("/api/photos", async (PhotoFileService photoFileService) =>
{
    try
    {
        var photos = photoFileService.GetAllPhotos();
        return Results.Ok(photos);
    }
    catch
    {
        return Results.StatusCode(500);
    }
});

// GET /api/photos/{fileName} - Get single photo with file path
app.MapGet("/api/photos/{fileName}", async (string fileName, PhotoFileService photoFileService) =>
{
    try
    {
        var photo = photoFileService.GetPhotoByFileName(fileName);
        if (photo == null)
        {
            return Results.NotFound(new { error = "Zdjęcie nie zostało znalezione" });
        }

        return Results.Ok(photo);
    }
    catch
    {
        return Results.StatusCode(500);
    }
});

// DELETE /api/photos/{fileName} - Delete a photo
app.MapDelete("/api/photos/{fileName}", async (string fileName) =>
{
    try
    {
        var filePath = Path.Combine("/photos", fileName);
        if (!File.Exists(filePath))
        {
            return Results.NotFound(new { error = "Zdjęcie nie zostało znalezione" });
        }

        File.Delete(filePath);
        return Results.Ok(new { message = "Zdjęcie zostało usunięte" });
    }
    catch
    {
        return Results.StatusCode(500);
    }
});

// POST /api/photos/album - Generate PDF album
app.MapPost("/api/photos/album", async ([FromBody] GenerateAlbumRequest request, PhotoFileService photoFileService, PdfService pdfService) =>
{
    if (request?.PhotoPaths == null || request.PhotoPaths.Length == 0)
    {
        return Results.BadRequest(new { error = "Nie wybrano żadnych zdjęć" });
    }

    try
    {
        var photos = photoFileService.GetPhotosByPaths(request.PhotoPaths);
        if (photos.Count == 0)
        {
            return Results.BadRequest(new { error = "Nie znaleziono żadnych zdjęć" });
        }

        var pdfBytes = await pdfService.GenerateAlbumAsync(photos);
        return Results.File(pdfBytes, "application/pdf", $"Album_Mai_{DateTime.Now:yyyyMMdd_HHmmss}.pdf");
    }
    catch
    {
        return Results.StatusCode(500);
    }
});

app.Run();