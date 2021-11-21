using System.Windows;

namespace TestData
{
	public class ThirdDependencyClass	:System.Windows.DependencyObject
	{
		#region ThirdProperty
		/// <summary>
		/// Third property.
		/// </summary>
		public string ThirdProperty
		{
			get
			{
				return (string) GetValue(ThirdPropertyProperty);
			}
			set
			{
				SetValue(ThirdPropertyProperty, value);
			}
		}

		/// <summary>
		/// Third property.
		/// </summary>
		public static readonly DependencyProperty ThirdPropertyProperty = DependencyProperty.Register(nameof(ThirdProperty), typeof(string), typeof(ThirdDependencyClass), new PropertyMetadata(null));
		#endregion

		#region First
		/// <summary>
		/// Reference to another object.
		/// </summary>
		public FirstDependencyClass First
		{
			get
			{
				return (FirstDependencyClass) GetValue(FirstProperty);
			}
			set
			{
				SetValue(FirstProperty, value);
			}
		}

		/// <summary>
		/// Reference to another object.
		/// </summary>
		public static readonly DependencyProperty FirstProperty = DependencyProperty.Register(nameof(First), typeof(FirstDependencyClass), typeof(ThirdDependencyClass), new PropertyMetadata(null));
		#endregion
	}
}
