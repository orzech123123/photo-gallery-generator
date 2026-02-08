using PhotoApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PhotoApp.Services;

public class PdfService
{
    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public async Task<byte[]> GenerateAlbumAsync(List<PhotoDto> photos, CancellationToken cancellationToken = default)
    {
        // Photos are already sorted by DateModified from PhotoFileService
        if (photos.Count == 0)
        {
            throw new InvalidOperationException("No photos found");
        }

        // Generate PDF
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(20);

                page.Content().Column(column =>
                {
                    // Process photos in pairs (2 per page)
                    for (int i = 0; i < photos.Count; i += 2)
                    {
                        if (i > 0)
                        {
                            column.Item().PageBreak();
                        }

                        column.Item().Row(row =>
                        {
                            // First photo
                            var photo1 = photos[i];
                            row.RelativeItem().Padding(5).Column(col =>
                            {
                                col.Item().Image(LoadImageFromFile(photo1.FilePath)).FitArea();
                                col.Item().PaddingTop(5).Text(photo1.FileName)
                                    .FontSize(10)
                                    .AlignCenter();
                                col.Item().Text(photo1.DateModified.ToLocalTime().ToString("yyyy-MM-dd HH:mm"))
                                    .FontSize(8)
                                    .AlignCenter()
                                    .FontColor(Colors.Grey.Darken2);
                            });

                            // Second photo (if exists)
                            if (i + 1 < photos.Count)
                            {
                                var photo2 = photos[i + 1];
                                row.RelativeItem().Padding(5).Column(col =>
                                {
                                    col.Item().Image(LoadImageFromFile(photo2.FilePath)).FitArea();
                                    col.Item().PaddingTop(5).Text(photo2.FileName)
                                        .FontSize(10)
                                        .AlignCenter();
                                    col.Item().Text(photo2.DateModified.ToLocalTime().ToString("yyyy-MM-dd HH:mm"))
                                        .FontSize(8)
                                        .AlignCenter()
                                        .FontColor(Colors.Grey.Darken2);
                                });
                            }
                            else
                            {
                                // Empty space if odd number of photos
                                row.RelativeItem();
                            }
                        });
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    private byte[] LoadImageFromFile(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                // Return a placeholder image if file doesn't exist
                return CreatePlaceholderImage();
            }

            return File.ReadAllBytes(filePath);
        }
        catch (Exception)
        {
            // Return placeholder on any error
            return CreatePlaceholderImage();
        }
    }

    private byte[] CreatePlaceholderImage()
    {
        // Create a simple 1x1 transparent PNG as placeholder
        return Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAADUlEQVR42mNkYPhfDwAChwGA60e6kgAAAABJRU5ErkJggg==");
    }
}