using System.IO;
using Microsoft.Maui.Controls;
using SkiaSharp;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис для обработки изображений обложек альбомов
/// </summary>
public class ImageService
{
    private const int TargetWidth = 200;
    private const int JpegQuality = 85;

    /// <summary>
    /// Выбрать изображение из файловой системы и выполнить ресайз
    /// </summary>
    public async Task<byte[]?> PickAndResizeImageAsync(CancellationToken cancellationToken = default)
    {
        var result = await FilePicker.PickAsync(new PickOptions
        {
            PickerTitle = "Выберите обложку альбома",
            FileTypes = FilePickerFileType.Images
        });

        if (result is null)
        {
            return null;
        }

        await using var stream = await result.OpenReadAsync();
        return await ResizeImageAsync(stream, TargetWidth, cancellationToken);
    }

    /// <summary>
    /// Выполнить ресайз изображения до указанной ширины с сохранением пропорций
    /// </summary>
    public async Task<byte[]?> ResizeImageAsync(Stream imageStream, int maxWidth = TargetWidth, CancellationToken cancellationToken = default)
    {
        using var memoryStream = new MemoryStream();
        await imageStream.CopyToAsync(memoryStream, cancellationToken);
        var data = memoryStream.ToArray();

        using var originalBitmap = SKBitmap.Decode(data);
        if (originalBitmap == null)
        {
            return null;
        }

        if (originalBitmap.Width <= maxWidth)
        {
            return EncodeBitmap(originalBitmap);
        }

        var ratio = (float)maxWidth / originalBitmap.Width;
        var targetHeight = (int)(originalBitmap.Height * ratio);

        using var resizedBitmap = new SKBitmap(maxWidth, targetHeight, originalBitmap.ColorType, originalBitmap.AlphaType);
        using (var canvas = new SKCanvas(resizedBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            var destRect = new SKRect(0, 0, maxWidth, targetHeight);
            canvas.DrawBitmap(originalBitmap, destRect);
        }

        return EncodeBitmap(resizedBitmap);
    }

    /// <summary>
    /// Получить ImageSource из массива байтов
    /// </summary>
    public ImageSource? GetImageSource(byte[]? imageBytes)
    {
        if (imageBytes is null || imageBytes.Length == 0)
        {
            return null;
        }

        return ImageSource.FromStream(() => new MemoryStream(imageBytes));
    }

    private static byte[] EncodeBitmap(SKBitmap bitmap)
    {
        using var image = SKImage.FromBitmap(bitmap);
        using var encoded = image.Encode(SKEncodedImageFormat.Jpeg, JpegQuality);
        return encoded.ToArray();
    }
}
