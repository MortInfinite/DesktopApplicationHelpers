using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Collections;

namespace GridViewTest
{
	/// <summary>
	/// Tests the DualList class, by adding a boolean checked state to each item in a collection.
	/// 
	/// This demonstrates how to add additional view-state data, based on items in a list, without modifying the list itself.
	/// </summary>
	public partial class DualListWindow :Window
	{
		public DualListWindow()
		{
			InitializeComponent();

			MyData = new ObservableCollection<MyDataType>
			{
				new MyDataType("A1", "B1", "C1"),
				new MyDataType("A2", "B2", "C2"),
				new MyDataType("A3", "B3", "C3")
			};
		}

		#region MyData
		/// <summary>
		/// List of items to base the DualList on.
		/// </summary>
		public IList<MyDataType> MyData
		{
			get
			{
				return (IList<MyDataType>) GetValue(MyDataProperty);
			}
			set
			{
				SetValue(MyDataProperty, value);
			}
		}

		/// <summary>
		/// List of items to base the DualList on.
		/// </summary>
		public static readonly DependencyProperty MyDataProperty = DependencyProperty.Register("MyData", typeof(IList<MyDataType>), typeof(DualListWindow), new PropertyMetadata(null, OnMyDataChanged));

		/// <summary>
		/// Creates a new DualList, when the <see cref="MyData"/> property is replaced.
		/// </summary>
		/// <param name="d">Object raising the event.</param>
		/// <param name="e">New value of the property.</param>
		private static void OnMyDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			DualListWindow		source		= (DualListWindow) d;
			IList<MyDataType>	newValue	= (IList<MyDataType>) e.NewValue;

			// Creates a new dual collection, with values that are initially set to false.
			// The secondary list indicates whether the item in the primary list is checked.
			DualList<MyDataType, bool> dualCollection = new DualList<MyDataType, bool>(newValue, source.IsChecked, (int index, MyDataType primaryValue)=>{return false;}, (int index, MyDataType primaryValue, bool currentValue)=>{return false;});
			source.DataContext = dualCollection;
		}
		#endregion

		#region IsChecked
		/// <summary>
		/// List of flags indicating whether each <see cref="MyData"/> item is checked.
		/// </summary>
		public IList<bool> IsChecked
		{
			get
			{
				return (IList<bool>) GetValue(IsCheckedProperty);
			}
			set
			{
				SetValue(IsCheckedProperty, value);
			}
		}

		/// <summary>
		/// List of flags indicating whether each <see cref="MyData"/> item is checked.
		/// </summary>
		public static readonly DependencyProperty IsCheckedProperty = DependencyProperty.Register("IsChecked", typeof(IList<bool>), typeof(DualListWindow), new PropertyMetadata(new ObservableCollection<bool>()));
		#endregion

		private void Button_Click(object sender, RoutedEventArgs e)
		{
			// Break the code such that the properties can be examined.
			Debugger.Break();
		}
	}
}
