using System;
using System.Windows.Media;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Converts a color into a hex code, containing Red, Green and Blue components (#RRGGBB).
	/// </summary>
	/// <example>
	/// <![CDATA[
	/// <TextBlock Foreground="{Binding Path=Color}" Text="{Binding Path=Color, Converter={StaticResource ResourceKey=ColorToRGBTextConverter}}"/>
	/// ]]>
	/// </example>
	public class ColorToRGBTextConverter	: IValueConverter
	{
		/// <summary>
		/// Converts a color into a hex code, containing Red, Green and Blue components (#RRGGBB).
		/// </summary>
		/// <param name="value">Color to convert to text.</param>
		/// <param name="targetType">Not used.</param>
		/// <param name="parameter">Not used.</param>
		/// <param name="culture">Not used.</param>
		/// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
		{
            if (null == value) 
                return null;

            if (value is Color) 
			{
                Color color = (Color)value;

				string result = string.Format("#{0:X2}{1:X2}{2:X2}", color.R, color.G, color.B);
                return result;
            }

            Type type = value.GetType();
            throw new InvalidOperationException("Unsupported type ["+type.Name+"]");            
        }

		/// <summary>
		/// Not supported.
		/// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
		{
            throw new NotSupportedException();
        }
	}
}
