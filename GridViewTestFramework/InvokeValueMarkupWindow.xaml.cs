using System.Collections.Generic;
using System.Windows;

namespace GridViewTest
{
	/// <summary>
	/// Interaction logic for InvokeValueMarkupWindow.xaml
	/// </summary>
	public partial class InvokeValueMarkupWindow :Window
	{
		public InvokeValueMarkupWindow()
		{
			MyData.Add(new MyDataType("A1", "B1", "C1"));
			MyData.Add(new MyDataType("A2", "B2", "C2"));
			MyData.Add(new MyDataType("A3", "B3", "C3"));

			InitializeComponent();
		}

		public object GetIsChecked(object dataContext)
		{
			IsCheckedDictionary.TryGetValue(dataContext, out bool isChecked);
			return isChecked;
		}

		public void SetIsChecked(object dataContext, object newValue)
		{
			IsCheckedDictionary[dataContext] = (bool) newValue;
		}

		public List<MyDataType> MyData
		{
			get
			{
				return (List<MyDataType>) GetValue(MyDataProperty);
			}
			set
			{
				SetValue(MyDataProperty, value);
			}
		}

		public Dictionary<object, bool> IsCheckedDictionary
		{
			get;
		} = new Dictionary<object, bool>();

		public static readonly DependencyProperty MyDataProperty = DependencyProperty.Register("MyData", typeof(List<MyDataType>), typeof(InvokeValueMarkupWindow), new PropertyMetadata(new List<MyDataType>()));


	}
}
