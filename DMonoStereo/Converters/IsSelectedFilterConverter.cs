using System.Globalization;
using DMonoStereo.Models;

namespace DMonoStereo.Converters;

public class IsSelectedFilterConverter : IMultiValueConverter
{
    public object? Convert(object[] values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values.Length >= 2 && 
            values[0] is MusicFilterOption currentItem && 
            values[1] is MusicFilterOption selectedItem)
        {
            return currentItem == selectedItem || 
                   (currentItem.Value == selectedItem.Value && 
                    currentItem.Title == selectedItem.Title &&
                    currentItem.Count == selectedItem.Count);
        }
        
        return false;
    }

    public object[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException("Обратная конвертация не поддерживается");
    }
}

