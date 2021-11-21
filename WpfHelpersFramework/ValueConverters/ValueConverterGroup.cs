using System;
using System.Collections.Generic;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Chains value converters together.
	/// </summary>
	public class ValueConverterGroup :List<IValueConverter>, IValueConverter
	{
		#region IValueConverter Members

		/// <summary>
		/// Chains a list of converters together, using the result of each converter as input argument to the next converter.
		/// </summary>
		public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			foreach(IValueConverter valueConverter in this)
				value = valueConverter.Convert(value, targetType, parameter, culture);

			return value;
		}

		/// <summary>
		/// Not implemented.
		/// </summary>
		public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}
}
