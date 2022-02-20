using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using Collections;

namespace GridViewTest
{
	/// <summary>
	/// Adds and removes a bunch of values to an observable list, using asynchronous tasks.
	/// </summary>
	public partial class ConcurrentObservableListWindow :Window
	{
		object lockObject = new object();
		public ConcurrentObservableListWindow()
		{
			InitializeComponent();

			const int maxValue = 10000;

			ConcurrentObservableList<int> collection = new ConcurrentObservableList<int>(true);
			
			// Inform WPF that the collection is multi threaded.
			BindingOperations.EnableCollectionSynchronization(collection, collection.SyncRoot);

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
			
			Task.Run(async ()=>
			{
				for(int count=0; count<maxValue; count+=5)
				{
					if(collection.Count % 5 == 0)
						collection.Clear();

					await Task.Delay(10);
				}
			});

			DataContext = collection;
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

		public static readonly DependencyProperty DataProperty = DependencyProperty.Register("Data", typeof(ICollection<int>), typeof(ConcurrentObservableListWindow), new PropertyMetadata(null));
	}
}
