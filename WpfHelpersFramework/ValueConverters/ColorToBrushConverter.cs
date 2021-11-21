using System;
using System.Windows.Media;
using System.Windows.Data;

namespace ValueConverters
{
	public class ColorToBrushConverter	: IValueConverter
	{
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
		{
            if (null == value) 
                return null;

            // For a more sophisticated converter, check also the targetType and react accordingly..
            if (value is Color) 
			{
                Color color = (Color)value;
                return new SolidColorBrush(color);
            }

            Type type = value.GetType();
            throw new InvalidOperationException("Unsupported type ["+type.Name+"]");            
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
		{
            throw new NotImplementedException();
        }
	}
}
