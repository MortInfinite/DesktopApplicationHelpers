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
	public class ConcurrentObservableCollectionTest
	{
		[TestMethod]
		public async Task Add()
		{
			const int							iterations			= 10000;
			ConcurrentObservableCollection<int> collection			= new ConcurrentObservableCollection<int>();
			ConcurrentBag<int>					notificationValues	= new ConcurrentBag<int>();
			int									countChanged		= 0;

			// Record every value provided by a collection changed event.
			collection.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				notificationValues.Add((int) e.NewItems[0]);
			};

			// Count number of times the Count property changes.
			collection.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableCollection<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			List<int> two = new List<int>();
			for(int count=0; count<iterations; count+=2)
				two.Add(count);

			List<int> three = new List<int>();
			for(int count=0; count<iterations; count+=3)
				three.Add(count);

			List<int> five = new List<int>();
			for(int count=0; count<iterations; count+=5)
				five.Add(count);

			// Add integers divisible by two.
			Task twoTask = Task.Run(async ()=>
			{
				Parallel.ForEach(two, (value)=>collection.Add(value));
				await Task.Delay(1);
			});

			// Add integers divisible by three.
			Task threeTask = Task.Run(async ()=>
			{
				Parallel.ForEach(three, (value)=>collection.Add(value));
				await Task.Delay(1);
			});

			// Add integers divisible by five.
			Task fiveTask = Task.Run(async ()=>
			{
				Parallel.ForEach(five, (value)=>collection.Add(value));
				await Task.Delay(1);
			});

			// Wait for all numbers to be added.
			await Task.WhenAll(twoTask, threeTask, fiveTask);

			// Check that the collection contains as many values as were added.
			Assert.IsTrue(collection.Count == two.Count+three.Count+five.Count);

			// Check that all added values are found in the collection.
			Parallel.ForEach(two,	(value)=>Assert.IsTrue(collection.Contains(value), $"The value {value} wasn't added from the {nameof(two)} list."));
			Parallel.ForEach(three,	(value)=>Assert.IsTrue(collection.Contains(value), $"The value {value} wasn't added from the {nameof(three)} list."));
			Parallel.ForEach(five,	(value)=>Assert.IsTrue(collection.Contains(value), $"The value {value} wasn't added from the {nameof(five)} list."));

			// Check that we received an event for each value that was added.
			Assert.IsTrue(notificationValues.Count == two.Count+three.Count+five.Count);

			// Check that an event was received for each value that was added.
			List<int> notificationValuesArray = notificationValues.ToList();
			foreach(int value in collection)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableCollection<int>.CollectionChanged)} event.");

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(two.Count+three.Count+five.Count, countChanged, $"The {nameof(ConcurrentObservableCollection<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableCollection<int>.Count)} property.");
		}

		[TestMethod]
		public void Remove()
		{
			const int							iterations					= 10000;
			ConcurrentObservableCollection<int> collection					= new ConcurrentObservableCollection<int>();
			ConcurrentBag<int>					notificationValuesRemoved	= new ConcurrentBag<int>();
			int									countChanged				= 0;

			// Record every value provided by a collection changed event.
			collection.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				switch(e.Action)
				{
					case NotifyCollectionChangedAction.Remove:
						notificationValuesRemoved.Add((int) e.OldItems[0]);
					break;
				}
			};

			// Count number of times the Count property changes.
			collection.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableCollection<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			List<int> two = new List<int>();
			for(int count=0; count<iterations; count+=2)
				two.Add(count);

			List<int> three = new List<int>();
			for(int count=0; count<iterations; count+=3)
				three.Add(count);

			// Add integers divisible by two.
			Parallel.ForEach(two, (value)=>collection.Add(value));

			// Remove integers divisible by three.
			Parallel.ForEach(three, (value)=>collection.Remove(value));

			// Check that all values from the two collection exists, unless it also exists in the three collection.
			foreach(int value in two)
			{
				if(three.Contains(value))
					Assert.IsFalse(collection.Contains(value), $"The value {value} wasn't removed from the collection.");
				else
					Assert.IsTrue(collection.Contains(value), $"The value {value} wasn't added to the collection or was removed although it shouldn't be.");
			}

			int numberOfThreesRemoved = 0;

			// Check that we received Remove events for every item removed.
			foreach(int value in three)
			{
				if(two.Contains(value))
				{
					Assert.IsTrue(notificationValuesRemoved.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableCollection<int>.CollectionChanged)} event.");
					numberOfThreesRemoved++;
				}
				else
					Assert.IsFalse(notificationValuesRemoved.Contains(value), $"The value {value} shouldn't have been raised by the {nameof(ConcurrentObservableCollection<int>.CollectionChanged)} event.");
			}

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(two.Count+numberOfThreesRemoved, countChanged, $"The {nameof(ConcurrentObservableCollection<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableCollection<int>.Count)} property.");
		}
	}
}
