using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Chains together a single multi value converter, followed by one or more value converter together.
	/// </summary>
	public class MultiValueConverterGroup :List<object>, IMultiValueConverter
	{
		#region IValueConverter Members

		/// <summary>
		/// Chains a list of converters together, using the result of each converter as input argument to the next converter.
		/// </summary>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			object value = null;

			foreach(object currentObject in this)
			{
				if(currentObject == null)
					continue;

				if(currentObject is IMultiValueConverter)
				{
					IMultiValueConverter multiValueConverter = (IMultiValueConverter) currentObject;
					value = multiValueConverter.Convert(values, targetType, parameter, culture);
				}
				else if(currentObject is IValueConverter)
				{
					IValueConverter valueConverter = (IValueConverter) currentObject;
					value = valueConverter.Convert(value, targetType, parameter, culture);
				}
				else
					throw new ArgumentException($"An element inside the {nameof(MultiValueConverterGroup)} was of type \"{currentObject.GetType()}\", which is neither an IValueConverter or an IMultiValueConverter.");
			}

			return value;
		}

		/// <summary>
		/// Not implemented.
		/// </summary>
		public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		#endregion
	}}
