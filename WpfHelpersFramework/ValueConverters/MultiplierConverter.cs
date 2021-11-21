using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Multiplies the input value with the converter parameter.
	/// </summary>
	public class MultiplierConverter :IValueConverter
	{
		/// <summary>
		/// Multiplies the value with the parameter.
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is double) || !(parameter is string))
				return value;

			double valueDouble		= (double) value;
			double parameterDouble;

			bool canParse = double.TryParse((string) parameter, NumberStyles.Float, CultureInfo.InvariantCulture, out parameterDouble);
			if(!canParse)
				return value;

			return valueDouble * parameterDouble;
		}

		/// <summary>
		/// Divides the value with the parameter.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if(!(value is double) || !(parameter is string))
				return value;

			double valueDouble		= (double) value;
			double parameterDouble;

			bool canParse = double.TryParse((string) parameter, NumberStyles.Float, CultureInfo.InvariantCulture, out parameterDouble);
			if(!canParse)
				return value;

			return valueDouble / parameterDouble;
		}
	}
}
