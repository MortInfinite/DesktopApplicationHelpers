using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Collections;

namespace GridViewTest
{
	/// <summary>
	/// Demonstrates a filtered list, which is based on a collection, in which items are being added by three asynchronous tasks.
	/// </summary>
	public partial class FilteredListWindow :Window
	{
		public FilteredListWindow()
		{
			InitializeComponent();

			const int maxValue = 1000;

			ConcurrentObservableCollection<int>		collection		= new ConcurrentObservableCollection<int>(true);

			Task.Run(async ()=>
			{
				for(int count=0; count<maxValue; count+=2)
				{
					collection.Add(count);

					await Task.Delay(10);
				}
			});

			Task.Run(async ()=>
			{
				for(int count=0; count<maxValue; count+=3)
				{
					collection.Add(count);

					await Task.Delay(10);
				}
			});


			Task.Run(async ()=>
			{
				for(int count=0; count<maxValue; count+=5)
				{
					collection.Add(count);

					await Task.Delay(10);
				}
			});

			// Wait for a few items to be added, such that the collection isn't empty to begin with.
			Thread.Sleep(50);

			ConcurrentObservableFilteredList<int>	filteredList	= new ConcurrentObservableFilteredList<int>(collection, Filter);
			DataContext = filteredList;
		}

		/// <summary>
		/// Filter out odd elements.
		/// </summary>
		/// <param name="element">Element to evaluate.</param>
		/// <returns>Returns true for even elements.</returns>
		private bool Filter(int element)
		{
			return element % 2 == 0;
		}

		public ICollection<int> Data
		{
			get
			{
				return (ICollection<int>) GetValue(DataProperty);
			}
			set
			{
				SetValue(DataProperty, value);
			}
		}

		public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(ICollection<int>), typeof(FilteredListWindow), new PropertyMetadata(null));
	}
}
