using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace AudioSplitter.Converters;
public class ProgressPanelBackgroundColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if ((bool)value)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#7F333333"));
        }
        return new SolidColorBrush(Colors.Transparent);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
