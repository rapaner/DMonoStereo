using SkiaSharp;

namespace DMonoStereo.Services;

/// <summary>
/// Сервис для обработки изображений обложек альбомов
/// </summary>
public class ImageService
{
    private const int TargetWidth = 300;
    private const int JpegQuality = 100;

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

        using var codecStream = new SKMemoryStream(data);
        using var codec = SKCodec.Create(codecStream);
        if (codec == null)
        {
            return null;
        }

        using var decodedBitmap = SKBitmap.Decode(codec);
        if (decodedBitmap == null)
        {
            return null;
        }

        using var orientedBitmap = NormalizeOrientation(decodedBitmap, codec.EncodedOrigin);

        if (orientedBitmap.Width <= maxWidth)
        {
            return EncodeBitmap(orientedBitmap);
        }

        var ratio = (float)maxWidth / orientedBitmap.Width;
        var targetHeight = Math.Max(1, (int)(orientedBitmap.Height * ratio));

        using var resizedBitmap = new SKBitmap(maxWidth, targetHeight, orientedBitmap.ColorType, orientedBitmap.AlphaType);
        using (var canvas = new SKCanvas(resizedBitmap))
        {
            canvas.Clear(SKColors.Transparent);
            var destRect = new SKRect(0, 0, maxWidth, targetHeight);
            canvas.DrawBitmap(orientedBitmap, destRect);
        }

        return EncodeBitmap(resizedBitmap);
    }

    private static SKBitmap NormalizeOrientation(SKBitmap bitmap, SKEncodedOrigin origin)
    {
        if (origin == SKEncodedOrigin.TopLeft || origin == SKEncodedOrigin.Default)
        {
            return bitmap.Copy();
        }

        var swapDimensions = origin is SKEncodedOrigin.LeftTop or SKEncodedOrigin.RightTop or SKEncodedOrigin.RightBottom or SKEncodedOrigin.LeftBottom;
        var width = swapDimensions ? bitmap.Height : bitmap.Width;
        var height = swapDimensions ? bitmap.Width : bitmap.Height;

        var rotated = new SKBitmap(width, height, bitmap.ColorType, bitmap.AlphaType);
        using var canvas = new SKCanvas(rotated);

        switch (origin)
        {
            case SKEncodedOrigin.TopRight:
                canvas.Translate(bitmap.Width, 0);
                canvas.Scale(-1, 1);
                break;

            case SKEncodedOrigin.BottomRight:
                canvas.Translate(bitmap.Width, bitmap.Height);
                canvas.RotateDegrees(180);
                break;

            case SKEncodedOrigin.BottomLeft:
                canvas.Translate(0, bitmap.Height);
                canvas.Scale(1, -1);
                break;

            case SKEncodedOrigin.LeftTop:
                canvas.Translate(0, bitmap.Width);
                canvas.RotateDegrees(270);
                canvas.Scale(-1, 1);
                break;

            case SKEncodedOrigin.RightTop:
                canvas.Translate(bitmap.Height, 0);
                canvas.RotateDegrees(90);
                break;

            case SKEncodedOrigin.RightBottom:
                canvas.Translate(bitmap.Height, bitmap.Width);
                canvas.RotateDegrees(90);
                canvas.Scale(-1, 1);
                break;

            case SKEncodedOrigin.LeftBottom:
                canvas.Translate(0, bitmap.Width);
                canvas.RotateDegrees(270);
                break;

            default:
                return bitmap.Copy();
        }

        canvas.DrawBitmap(bitmap, 0, 0);
        return rotated;
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