using Avalonia.Data.Converters;

using System.Diagnostics;
using System.Globalization;

namespace One.Toolbox.Converters;

public class DebugConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        // 在调试器中查看 value 的值和类型
        Debug.WriteLine($"Binding Value: {value}, Type: {value?.GetType().Name}");
        return value; // 不改变原始值
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}