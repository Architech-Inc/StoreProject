using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

namespace Store.API.Infrastructure.Processing;

public interface IImageProcessorService
{
    Task<(Stream ThumbnailStream, Stream FullStream)> ProcessImageAsync(Stream sourceStream, SixLabors.ImageSharp.Rectangle? cropArea = null, CancellationToken ct = default);
}

public class ImageProcessorService : IImageProcessorService
{
    private const int ThumbnailMaxWidth = 200;
    private const int ThumbnailMaxHeight = 200;
    private const int FullMaxWidth = 800;
    private const int FullMaxHeight = 800;

    public async Task<(Stream ThumbnailStream, Stream FullStream)> ProcessImageAsync(Stream sourceStream, SixLabors.ImageSharp.Rectangle? cropArea = null, CancellationToken ct = default)
    {
        // Load the image (this automatically strips EXIF data if we don't preserve it explicitly when saving)
        using var image = await Image.LoadAsync(sourceStream, ct);

        // Remove EXIF and other metadata
        image.Metadata.ExifProfile = null;
        image.Metadata.IccProfile = null;
        image.Metadata.XmpProfile = null;

        // Ensure streams are ready
        var thumbStream = new MemoryStream();
        var fullStream = new MemoryStream();

        var webpEncoder = new WebpEncoder { Quality = 80 };

        // Generate Full Image (Uncropped)
        using (var fullImage = image.Clone(x => x.Resize(new ResizeOptions
        {
            Size = new Size(FullMaxWidth, FullMaxHeight),
            Mode = ResizeMode.Max
        })))
        {
            await fullImage.SaveAsWebpAsync(fullStream, webpEncoder, ct);
            fullStream.Position = 0;
        }

        // Generate Thumbnail Image
        using (var thumbImage = image.Clone(x =>
        {
            // If the user provided a crop area, apply it first
            if (cropArea.HasValue)
            {
                // Ensure the crop area is within the bounds of the image
                var crop = cropArea.Value;
                crop.Intersect(new SixLabors.ImageSharp.Rectangle(0, 0, image.Width, image.Height));
                
                if (crop.Width > 0 && crop.Height > 0)
                {
                    x.Crop(crop);
                }
            }
            
            x.Resize(new ResizeOptions
            {
                Size = new Size(ThumbnailMaxWidth, ThumbnailMaxHeight),
                Mode = ResizeMode.Max
            });
        }))
        {
            await thumbImage.SaveAsWebpAsync(thumbStream, webpEncoder, ct);
            thumbStream.Position = 0;
        }

        return (thumbStream, fullStream);
    }
}
