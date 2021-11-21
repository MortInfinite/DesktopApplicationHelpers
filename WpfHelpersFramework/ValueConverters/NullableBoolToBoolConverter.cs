using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Converts a nullable bool (bool?) into a bool type.
	/// </summary>
	/// <remarks>
	/// This converter is used to convert the IsChecked value, from a checkbox, into a boolean value.
	/// </remarks>
	public class NullableBoolToBoolConverter:IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is bool?))
				return value;

			if((bool?) value == true)
				return true;

			return false;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return value;
		}
	}
}
