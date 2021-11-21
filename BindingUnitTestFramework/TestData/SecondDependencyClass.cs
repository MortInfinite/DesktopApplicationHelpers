using System.Windows;

namespace TestData
{
	public class SecondDependencyClass	:System.Windows.DependencyObject
	{
		#region SecondProperty
		/// <summary>
		/// Second property.
		/// </summary>
		public string SecondProperty
		{
			get
			{
				return (string) GetValue(SecondPropertyProperty);
			}
			set
			{
				SetValue(SecondPropertyProperty, value);
			}
		}

		/// <summary>
		/// Second property.
		/// </summary>
		public static readonly DependencyProperty SecondPropertyProperty = DependencyProperty.Register(nameof(SecondProperty), typeof(string), typeof(SecondDependencyClass), new PropertyMetadata(null));
		#endregion

		#region Third
		/// <summary>
		/// Reference to another object.
		/// </summary>
		public ThirdDependencyClass Third
		{
			get
			{
				return (ThirdDependencyClass) GetValue(ThirdProperty);
			}
			set
			{
				SetValue(ThirdProperty, value);
			}
		}

		/// <summary>
		/// Reference to another object.
		/// </summary>
		public static readonly DependencyProperty ThirdProperty = DependencyProperty.Register(nameof(Third), typeof(ThirdDependencyClass), typeof(ThirdDependencyClass), new PropertyMetadata(null));
		#endregion
	}
}
