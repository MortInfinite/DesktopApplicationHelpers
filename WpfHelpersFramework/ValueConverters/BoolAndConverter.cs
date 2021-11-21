using System;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Performs a boolean "and" operation on all values.
	/// </summary>
	public class BoolAndConverter :IMultiValueConverter
	{
		/// <summary>
		/// Ands all values together.
		/// </summary>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			foreach(object value in values)
			{
				if(!(value is bool))
					continue;

				bool boolValue = (bool) value;
				if(!boolValue)
					return false;
			}

			return true;
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
