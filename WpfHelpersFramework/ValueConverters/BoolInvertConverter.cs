using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Flips the value of a bool, from true to false or false to true.
	/// </summary>
	public class BoolInvertConverter :IValueConverter
	{
		/// <summary>
		/// Flips the value of a bool, from true to false or false to true.
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is bool))
				return value;

			return !((bool) value);
		}

		/// <summary>
		/// Flips the value of a bool, from true to false or false to true.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is bool))
				return value;

			return !((bool) value);
		}
	}
}
