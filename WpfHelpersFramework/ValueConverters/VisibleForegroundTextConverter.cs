using System;
using System.Windows.Media;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Determines which foreground text color to show, depending on a specified background solid color brush.
	/// </summary>
	/// <example>
	/// <![CDATA[
	/// <TextBlock Foreground="{Binding Path=Color, Converter={StaticResource ResourceKey=VisibleForegroundTextConverter}}" Text="Example text"/>
	/// ]]>
	/// </example>
	public class VisibleForegroundTextConverter	: IValueConverter
	{
		/// <summary>
		/// Returns a <see cref="Colors.Black"/> brush if the specified color has any color component that is 
		/// more than 0x80 in value. Otherwise returns a <see cref="Colors.White"/> brush.
		/// 
		/// This converter is intended to show a text that is visible, based on the background color, being used
		/// behind the text.
		/// </summary>
		/// <param name="value">Background color used to determine whether to return a black or a white brush.</param>
		/// <param name="targetType">Not used.</param>
		/// <param name="parameter">Not used.</param>
		/// <param name="culture">Not used.</param>
		/// <returns></returns>
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
		{
            if (null == value) 
                return null;

            // For a more sophisticated converter, check also the targetType and react accordingly..
            if (value is Color) 
			{
                Color color = (Color)value;

				if(color.R>0x80 || color.G>0x80 || color.B>0x80)
	                return new SolidColorBrush(Colors.Black);

                return new SolidColorBrush(Colors.White);
            }

            Type type = value.GetType();
            throw new InvalidOperationException("Unsupported type ["+type.Name+"]");            
        }

		/// <summary>
		/// This method is not supported.
		/// </summary>
        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture) 
		{
            throw new NotSupportedException();
        }
	}
}
