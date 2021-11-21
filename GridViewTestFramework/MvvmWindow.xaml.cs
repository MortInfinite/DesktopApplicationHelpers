using System.Windows;
using Bindings;

namespace GridViewTest
{
	/// <summary>
	/// Demonstrates how to use hierarchical bindings to observe chains of objects.
	/// </summary>
	public partial class MvvmWindow :Window
	{
		public MvvmWindow()
		{
			InitializeComponent();

			// Listen to changes in the the DataContext property's First property and sets them on this class' First property.
			m_firstBinding	= new HierarchicalBinding(this, "DataContext.First", this, nameof(First), BindingModes.OneWay);

			// Listen to changes in the the DataContext property's Second property and sets them on this class' Second property.
			m_secondBinding	= new HierarchicalBinding(this, "DataContext.Second", this, nameof(Second), BindingModes.TwoWay);
		}

		#region First
		/// <summary>
		/// First property.
		/// </summary>
		public string First
		{
			get
			{
				return (string) GetValue(FirstProperty);
			}
			set
			{
				SetValue(FirstProperty, value);
			}
		}

		/// <summary>
		/// First property.
		/// </summary>
		public static readonly DependencyProperty FirstProperty = DependencyProperty.Register(nameof(First), typeof(string), typeof(MvvmWindow), new PropertyMetadata(null, FirstPropertyChanged));

		private static void FirstPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}
		#endregion

		#region Second
		/// <summary>
		/// Second property.
		/// </summary>
		public string Second
		{
			get
			{
				return (string) GetValue(SecondProperty);
			}
			set
			{
				SetValue(SecondProperty, value);
			}
		}

		/// <summary>
		/// Second property.
		/// </summary>
		public static readonly DependencyProperty SecondProperty = DependencyProperty.Register(nameof(Second), typeof(string), typeof(MvvmWindow), new PropertyMetadata(null, SecondPropertyChanged));

		private static void SecondPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
		}
		#endregion

		HierarchicalBinding m_firstBinding,
							m_secondBinding;
	}
}
