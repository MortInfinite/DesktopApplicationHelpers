using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Replaces double.NaN values with the parameter value or 0.0 if no parameter is specified.
	/// </summary>
	public class NanProtectorConverter :IValueConverter
	{
		/// <summary>
		/// Replaces double.NaN values with the parameter value or 0.0 if no parameter is specified.
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is double doubleValue))
				return value;

			if(double.IsNaN(doubleValue))
			{
				if(parameter is double parameterDouble)
					return parameter;

				return 0.0;
			}

			return doubleValue;
		}

		/// <summary>
		/// Replaces double.NaN values with the parameter value or 0.0 if no parameter is specified.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is double doubleValue))
				return value;

			if(double.IsNaN(doubleValue))
			{
				if(parameter is double parameterDouble)
					return parameter;

				return 0.0;
			}

			return doubleValue;
		}
	}
}
