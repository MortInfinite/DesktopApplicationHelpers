using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace ValueConverters
{
	/// <summary>
	/// Converts the top coordinate from the relativeTo coordinate system to the source coordinate system.
	/// </summary>
	public class TransformXConverter :IMultiValueConverter
	{
		/// <summary>
		/// Translates the X coordiante, from values[2], into the coordinate system of the control in values[0],
		/// from the coordinate system from the control in values[1].
		/// </summary>
		/// <param name="values">
		/// values[0] = Source container control, containing the coordinate.
		/// values[1] = Target container control, into which the coordinate is transformed.
		/// values[2] = X coordinate to transform from the source container into the target container.
		/// </param>
		/// <param name="targetType">Not used.</param>
		/// <param name="parameter">Not used.</param>
		/// <param name="culture">Not used.</param>
		/// <returns>X coordinate, in the target container (values[1]).</returns>
		public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
		{
			if(values.Length != 3)
				throw new ArgumentException("The TransformMultiValueConverter expects three input values.");
			UIElement	source		= values[0] as UIElement;
			UIElement	relativeTo	= values[1] as UIElement;
			double?		left		= values[2] as double?;

			if(source == null)
				throw new ArgumentException("The first value must contain a UIElement.");
			if(relativeTo == null)
				throw new ArgumentException("The second value must contain a UIElement.");
			if(left == null)
				throw new ArgumentException("The third value must contain a double.");

			// Convert the local coordinate of the dragged thumb to coordinates inside the MainCanvas.
			Point point = source.TranslatePoint(new Point(left.Value, 0), relativeTo);

			return point.X;
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
