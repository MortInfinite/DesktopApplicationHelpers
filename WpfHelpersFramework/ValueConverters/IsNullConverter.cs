using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Determines if the specified value is null.
	/// </summary>
	public class IsNullConverter :IValueConverter
	{
		/// <summary>
		/// If the value is null, return true.
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value == null;
		}

		/// <summary>
		/// If the value is true, return null.
		/// If the value is false, return false.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is bool))
				return value;

			// If the value is true, return null.
			if(((bool) value) == true)
				return null;

			// Return a non-null value.
			return false;
		}
	}
}
