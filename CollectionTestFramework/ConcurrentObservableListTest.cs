using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Specialized;
using System.Threading;

namespace CollectionTest
{
	[TestClass]
	public class ConcurrentObservableListTest
	{
		[TestMethod]
		public void Move()
		{
			const int						iterations			= 10000;
			ConcurrentObservableList<int>	list				= new ConcurrentObservableList<int>();
			ConcurrentBag<int>				notificationValues	= new ConcurrentBag<int>();

			// Record every move event.
			list.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Move)
					notificationValues.Add((int) e.NewItems[0]);
			};

			// Add positive values to the list.
			for(int count=0; count<iterations; count++)
				list.Add(count);

			// Add positive values to the list, after the added positive numbers.
			for(int count=0; count<iterations; count++)
				list.Add(-count);

			// Check that all added values are found in the list.
			Parallel.For(0, iterations-1, count => {Assert.AreEqual(count, list[count], $"The value at index {count} was expected to be {count}, but was {list[count]}.");});
			Parallel.For(0, iterations-1, count => {Assert.AreEqual(-count, list[iterations+count], $"The value at index {iterations+count} was expected to be {-count}, but was {list[iterations+count]}.");});

			// Swap positive and negative numbers.
			for(int count=0; count<iterations; count++)
			{
				list.Move(count, iterations+count-1);
				list.Move(iterations+count, count);
			}

			// Check that all added values are found in the list.
			Parallel.For(0, iterations-1, count => {Assert.AreEqual(-count, list[count], $"The value at index {count} was expected to be {count}, but was {list[count]}.");});
			Parallel.For(0, iterations-1, count => {Assert.AreEqual(count, list[iterations+count], $"The value at index {iterations+count} was expected to be {-count}, but was {list[iterations+count]}.");});

			// Check that we received an event for each value that was moved.
			Assert.IsTrue(notificationValues.Count == iterations*2);
		}

		/// <summary>
		/// Test that the event subscriber prevents other threads from accessing the contents of the list, before the event subscription has returned.
		/// 
		/// This is necessary to allow subscribers to examine an item added, before another thread has a chance to remove it again.
		/// </summary>
		[TestMethod]
		public void AddAndClearDuringNotify()
		{
			const int						iterations			= 10000;
			ConcurrentObservableList<int>	list				= new ConcurrentObservableList<int>(true, false);

			// Slow event subscriber.
			list.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
				{ 
					// This will fail if the parallel threads are allowed to clear the list, before the event handler returns.
					int addedItem = list[e.NewStartingIndex];

					Assert.IsTrue(list.Contains(e.NewItems[0]), $"The added item was removed from the list before the event subscriber had a chance to examine it.");
				}
			};

			// Add a bunch of values and clear them again, before the event subscriber has a chance to retrieve the value.
			Parallel.For(0, iterations-1, count => 
			{
				list.Add(count);
				list.Clear();
			});
		}
	}
}
