using System.Globalization;

namespace DMonoStereo.Converters;

public class ByteArrayToImageSourceConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is byte[] bytes && bytes.Length > 0)
        {
            // Создаем копию массива для безопасной работы с потоком
            var imageBytes = new byte[bytes.Length];
            Array.Copy(bytes, imageBytes, bytes.Length);
            return ImageSource.FromStream(() => new MemoryStream(imageBytes));
        }

        return null;
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Конвертация ImageSource в byte[] не поддерживается");
    }
}