using System.Windows;

namespace TestData
{
	/// <summary>
	/// Test dependency object that has a value and a reference to another object.
	/// </summary>
	public class TestDependencyObject	:DependencyObject
	{
		#region FirstProperty
		/// <summary>
		/// String value.
		/// </summary>
		public string Value
		{
			get
			{
				return (string) GetValue(ValueProperty);
			}
			set
			{
				SetValue(ValueProperty, value);
			}
		}

		/// <summary>
		/// First property.
		/// </summary>
		public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(TestDependencyObject), new PropertyMetadata(null));
		#endregion

		#region Second
		/// <summary>
		/// Reference to another object.
		/// </summary>
		public object Other
		{
			get
			{
				return (object) GetValue(OtherProperty);
			}
			set
			{
				SetValue(OtherProperty, value);
			}
		}

		/// <summary>
		/// Reference to another object.
		/// </summary>
		public static readonly DependencyProperty OtherProperty = DependencyProperty.Register(nameof(Other), typeof(object), typeof(TestDependencyObject), new PropertyMetadata(null));
		#endregion
	}
}
