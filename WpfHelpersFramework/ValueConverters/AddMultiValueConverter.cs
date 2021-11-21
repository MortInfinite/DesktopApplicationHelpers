using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Add multiple double values together.
	/// </summary>
	public class AddMultiValueConverter :IMultiValueConverter
	{
		/// <summary>
		/// Adds all values together.
		/// </summary>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			double result = 0;

			foreach(object value in values)
			{
				if(!(value is double))
					continue;

				double doubleValue = (double) value;

				if(double.IsNaN(doubleValue))
					continue;

				result += doubleValue;
			}

			return result;
		}

		/// <summary>
		/// Not implemented.
		/// </summary>
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}
	}
}
