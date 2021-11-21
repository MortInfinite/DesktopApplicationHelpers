using System.Windows;

namespace TestData
{
	public class FirstDependencyClass	:System.Windows.DependencyObject
	{
		#region FirstProperty
		/// <summary>
		/// First property.
		/// </summary>
		public string FirstProperty
		{
			get
			{
				return (string) GetValue(FirstPropertyProperty);
			}
			set
			{
				SetValue(FirstPropertyProperty, value);
			}
		}

		/// <summary>
		/// First property.
		/// </summary>
		public static readonly DependencyProperty FirstPropertyProperty = DependencyProperty.Register(nameof(FirstProperty), typeof(string), typeof(FirstDependencyClass), new PropertyMetadata(null));
		#endregion

		#region Second
		/// <summary>
		/// Reference to another object.
		/// </summary>
		public SecondDependencyClass Second
		{
			get
			{
				return (SecondDependencyClass) GetValue(SecondProperty);
			}
			set
			{
				SetValue(SecondProperty, value);
			}
		}

		/// <summary>
		/// Reference to another object.
		/// </summary>
		public static readonly DependencyProperty SecondProperty = DependencyProperty.Register(nameof(Second), typeof(SecondDependencyClass), typeof(FirstDependencyClass), new PropertyMetadata(null));
		#endregion
	}
}
