using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Converts a bool to a Visibility.
	/// </summary>
	public class BoolToVisibilityConverter :IValueConverter
	{
		/// <summary>
		/// Converts a bool to a Visibility.
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is bool))
				return value;

			if((bool) value)
				return Visibility.Visible;

			return Visibility.Collapsed;
		}

		/// <summary>
		/// Converts Visibility to a bool.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is Visibility))
				return value;

			if((Visibility) value == Visibility.Visible)
				return true;

			return false;
		}
	}
}
