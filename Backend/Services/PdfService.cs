using PhotoApp.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace PhotoApp.Services;

public class PdfService
{
    private readonly Random _random;

    public PdfService()
    {
        QuestPDF.Settings.License = LicenseType.Community;
        // Use time-based seed for better randomness
        _random = new Random(Guid.NewGuid().GetHashCode());
        Console.WriteLine("PdfService initialized with new Random seed");
    }

    public async Task<byte[]> GenerateAlbumAsync(List<PhotoDto> photos, CancellationToken cancellationToken = default)
    {
        // Photos are already sorted by DateModified from PhotoFileService
        if (photos.Count == 0)
        {
            throw new InvalidOperationException("No photos found");
        }

        Console.WriteLine($"Starting PDF generation with {photos.Count} photos");

        // Generate PDF with random layouts
        var document = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(10);

                page.Content().AlignCenter().AlignMiddle().Column(column =>
                {
                    int currentIndex = 0;
                    int pageNumber = 1;

                    while (currentIndex < photos.Count)
                    {
                        if (currentIndex > 0)
                        {
                            column.Item().PageBreak();
                        }

                        int remainingPhotos = photos.Count - currentIndex;
                        int photosOnThisPage = DeterminePhotosPerPage(remainingPhotos);

                        Console.WriteLine($"Page {pageNumber}: Processing {photosOnThisPage} photos (index {currentIndex})");
                        GeneratePageLayout(column, photos, currentIndex, photosOnThisPage);
                        currentIndex += photosOnThisPage;
                        pageNumber++;
                    }
                });
            });
        });

        return document.GeneratePdf();
    }

    private int DeterminePhotosPerPage(int remainingPhotos)
    {
        if (remainingPhotos == 1) return 1;
        if (remainingPhotos == 2) return 2;

        // For 3 or more photos, prefer 3 photos for more variety
        // 70% chance for 3 photos, 30% chance for 2 photos
        int randomRoll = _random.Next(1, 11);
        int result = randomRoll <= 7 ? 3 : 2;

        // Make sure we don't exceed remaining photos
        if (result > remainingPhotos) result = remainingPhotos;

        Console.WriteLine($"Remaining: {remainingPhotos}, Random roll: {randomRoll}, Chosen: {result}");
        return result;
    }

    private void GeneratePageLayout(ColumnDescriptor column, List<PhotoDto> photos, int startIndex, int photoCount)
    {
        switch (photoCount)
        {
            case 1:
                GenerateSinglePhotoLayout(column, photos[startIndex]);
                break;
            case 2:
                GenerateTwoPhotoLayout(column, photos[startIndex], photos[startIndex + 1]);
                break;
            case 3:
                GenerateThreePhotoLayout(column, photos[startIndex], photos[startIndex + 1], photos[startIndex + 2]);
                break;
        }
    }

    private void GenerateSinglePhotoLayout(ColumnDescriptor column, PhotoDto photo)
    {
        column.Item().AlignMiddle().Padding(2).Image(LoadImageFromFile(photo.FilePath)).FitArea();
    }

    private void GenerateTwoPhotoLayout(ColumnDescriptor column, PhotoDto photo1, PhotoDto photo2)
    {
        column.Item().Row(row =>
        {
            row.RelativeItem(0.5f).AlignMiddle().Padding(2).Image(LoadImageFromFile(photo1.FilePath)).FitArea();
            row.RelativeItem(0.5f).AlignMiddle().Padding(2).Image(LoadImageFromFile(photo2.FilePath)).FitArea();
        });
    }

    private void GenerateThreePhotoLayout(ColumnDescriptor column, PhotoDto photo1, PhotoDto photo2, PhotoDto photo3)
    {
        // Use a more balanced 3-photo grid layout
        // All photos get equal visual weight
        column.Item().Row(row =>
        {
            // Create three equal columns for balanced layout
            row.RelativeItem(0.33f).AlignMiddle().Padding(2).Image(LoadImageFromFile(photo1.FilePath)).FitArea();
            row.RelativeItem(0.33f).AlignMiddle().Padding(2).Image(LoadImageFromFile(photo2.FilePath)).FitArea();
            row.RelativeItem(0.33f).AlignMiddle().Padding(2).Image(LoadImageFromFile(photo3.FilePath)).FitArea();
        });
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