using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Specialized;
using System.Threading;
using System;

namespace CollectionTest
{
	[TestClass]
	public class ConcurrentObservableSortedListTest
	{
		[TestMethod]
		public async Task Add()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableSortedList<int>		sortedList			= new ConcurrentObservableSortedList<int>(collection, (int first, int second)=>first.CompareTo(second));
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			int										countChanged		= 0;
			const int								iterations			= 10000;

			// Record every value provided by a collection changed event.
			sortedList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				notificationValues.Add((int) e.NewItems[0]);
			};

			// Count number of times the Count property changes.
			sortedList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			List<int> two			= new List<int>();
			for(int count=0; count<iterations; count+=2)
				two.Add(count);

			List<int> three			= new List<int>();
			for(int count=0; count<iterations; count+=3)
				three.Add(count);

			List<int> five			= new List<int>();
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
			Assert.IsTrue(sortedList.Count == two.Count+three.Count+five.Count);

			// Check that all added values are found in the collection.
			Parallel.ForEach(two,	(value)=>Assert.IsTrue(sortedList.Contains(value), $"The value {value} wasn't added from the {nameof(two)} list."));
			Parallel.ForEach(three,	(value)=>Assert.IsTrue(sortedList.Contains(value), $"The value {value} wasn't added from the {nameof(three)} list."));
			Parallel.ForEach(five,	(value)=>Assert.IsTrue(sortedList.Contains(value), $"The value {value} wasn't added from the {nameof(five)} list."));

			// Check that we received an event for each value that was added.
			Assert.IsTrue(notificationValues.Count == two.Count+three.Count+five.Count);

			// Check that an event was received for each value that was added.
			List<int> notificationValuesArray = notificationValues.ToList();
			foreach(int value in two)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in three)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in five)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(two.Count+three.Count+five.Count, countChanged, $"The {nameof(ConcurrentObservableFilteredList<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableFilteredList<int>.Count)} property.");

			// Check that all items are sorted.
			for(int count=1; count<sortedList.Count; count++)
				Assert.IsTrue(sortedList[count] >= sortedList[count-1], $"Item at index {count} was not correctly sorted, its value was {sortedList[count]} but the previous index was {sortedList[count-1]}");
		}

		[TestMethod]
		public async Task AddRandom()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableSortedList<int>		sortedList			= new ConcurrentObservableSortedList<int>(collection, (int first, int second)=>first.CompareTo(second));
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			int										countChanged		= 0;
			const int								iterations			= 10000;
			Random									random				= new Random(7);

			// Record every value provided by a collection changed event.
			sortedList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				notificationValues.Add((int) e.NewItems[0]);
			};

			// Count number of times the Count property changes.
			sortedList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			List<int> firstList			= new List<int>();
			for(int count=0; count<iterations; count++)
				firstList.Add(random.Next(0, iterations*10));

			List<int> secondList			= new List<int>();
			for(int count=0; count<iterations; count++)
				secondList.Add(random.Next(0, iterations*10));

			List<int> thirdList			= new List<int>();
			for(int count=0; count<iterations; count++)
				thirdList.Add(random.Next(0, iterations*10));

			// Add integers divisible by two.
			Task twoTask = Task.Run(async ()=>
			{
				Parallel.ForEach(firstList, (value)=>collection.Add(value));
				await Task.Delay(1);
			});

			// Add integers divisible by three.
			Task threeTask = Task.Run(async ()=>
			{
				Parallel.ForEach(secondList, (value)=>collection.Add(value));
				await Task.Delay(1);
			});

			// Add integers divisible by five.
			Task fiveTask = Task.Run(async ()=>
			{
				Parallel.ForEach(thirdList, (value)=>collection.Add(value));
				await Task.Delay(1);
			});

			// Wait for all numbers to be added.
			await Task.WhenAll(twoTask, threeTask, fiveTask);

			// Check that the collection contains as many values as were added.
			Assert.IsTrue(sortedList.Count == firstList.Count+secondList.Count+thirdList.Count);

			// Check that all added values are found in the collection.
			Parallel.ForEach(firstList,	(value)=>Assert.IsTrue(sortedList.Contains(value), $"The value {value} wasn't added from the {nameof(firstList)} list."));
			Parallel.ForEach(secondList,	(value)=>Assert.IsTrue(sortedList.Contains(value), $"The value {value} wasn't added from the {nameof(secondList)} list."));
			Parallel.ForEach(thirdList,	(value)=>Assert.IsTrue(sortedList.Contains(value), $"The value {value} wasn't added from the {nameof(thirdList)} list."));

			// Check that we received an event for each value that was added.
			Assert.IsTrue(notificationValues.Count == firstList.Count+secondList.Count+thirdList.Count);

			// Check that an event was received for each value that was added.
			List<int> notificationValuesArray = notificationValues.ToList();
			foreach(int value in firstList)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in secondList)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in thirdList)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(firstList.Count+secondList.Count+thirdList.Count, countChanged, $"The {nameof(ConcurrentObservableFilteredList<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableFilteredList<int>.Count)} property.");

			// Check that all items are sorted.
			for(int count=1; count<sortedList.Count; count++)
				Assert.IsTrue(sortedList[count] >= sortedList[count-1], $"Item at index {count} was not correctly sorted, its value was {sortedList[count]} but the previous index was {sortedList[count-1]}");
		}

		[TestMethod]
		public async Task Remove()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableSortedList<int>		sortedList			= new ConcurrentObservableSortedList<int>(collection, (int first, int second)=>first.CompareTo(second));
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			int										countChanged		= 0;
			const int								iterations			= 10000;

			// Record every value provided by a collection changed event.
			sortedList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
					notificationValues.Add((int) e.NewItems[0]);
				else if(e.Action == NotifyCollectionChangedAction.Remove)
					notificationValues.Add((int) e.OldItems[0]);
			};

			// Count number of times the Count property changes.
			sortedList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			List<int> five			= new List<int>();
			for(int count=0; count<iterations; count+=5)
				five.Add(count);

			List<int> six			= new List<int>();
			for(int count=0; count<iterations; count+=6)
				six.Add(count);

			// Add integers divisible by five.
			Task fiveTask = Task.Run(async ()=>
			{
				Parallel.ForEach(five, (value)=>collection.Add(value));
				await Task.Delay(1);
			});

			// Add integers divisible by six.
			Task sixTask = Task.Run(async ()=>
			{
				Parallel.ForEach(six, (value)=>collection.Add(value));
				await Task.Delay(1);
			});

			// Wait for all numbers to be added.
			await Task.WhenAll(fiveTask, sixTask);

			// Remove interegers divisible by six.
			Task removeSixTask = Task.Run(()=>
			{
				Parallel.ForEach(six, (value)=>collection.Remove(value));
			});

			// Wait for numbers to be removed.
			await removeSixTask;

			// Check that the collection contains as many values as were added.
			Assert.IsTrue(sortedList.Count == five.Count);

			// Check that all added values are found in the collection.
			Parallel.ForEach(five,	(value)=>Assert.IsTrue(sortedList.Contains(value), $"The value {value} wasn't added from the {nameof(five)} list."));

			// Check that we received an event for each value that was added.
			Assert.IsTrue(notificationValues.Count == five.Count+six.Count+six.Count);

			// Check that an event was received for each value that was added.
			List<int> notificationValuesArray = notificationValues.ToList();
			foreach(int value in five)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in six)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(five.Count+six.Count+six.Count, countChanged, $"The {nameof(ConcurrentObservableFilteredList<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableFilteredList<int>.Count)} property.");

			// Check that all items are sorted.
			for(int count=1; count<sortedList.Count; count++)
				Assert.IsTrue(sortedList[count] >= sortedList[count-1], $"Item at index {count} was not correctly sorted, its value was {sortedList[count]} but the previous index was {sortedList[count-1]}");
		}

		[TestMethod]
		public async Task Clear()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableSortedList<int>		sortedList			= new ConcurrentObservableSortedList<int>(collection, (int first, int second)=>first.CompareTo(second));
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			int										clearCount			= 0;
			int										countChanged		= 0;
			const int								iterations			= 10000;

			// Record every value provided by a collection changed event.
			sortedList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
					notificationValues.Add((int) e.NewItems[0]);
				else if(e.Action == NotifyCollectionChangedAction.Reset)
					clearCount++;
			};

			// Count number of times the Count property changes.
			sortedList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			List<int> two			= new List<int>();
			for(int count=0; count<iterations; count+=2)
				two.Add(count);

			List<int> three			= new List<int>();
			for(int count=0; count<iterations; count+=3)
				three.Add(count);

			List<int> five			= new List<int>();
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

			// Clear the collection.
			Task fiveTask = Task.Run(async ()=>
			{
				Parallel.ForEach(five, (value)=>collection.Clear());
				await Task.Delay(1);
			});


			// Wait for all numbers to be added.
			await Task.WhenAll(twoTask, threeTask, fiveTask);

			// Check that we received an event for each value that was added.
			Assert.IsTrue(notificationValues.Count == two.Count+three.Count);

			// Check that we received an event every time the collection was cleared.
			Assert.IsTrue(clearCount == five.Count);

			// Check that an event was received for each value that was added.
			List<int> notificationValuesArray = notificationValues.ToList();
			foreach(int value in two)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in three)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.IsTrue(countChanged > two.Count+three.Count, $"The {nameof(ConcurrentObservableFilteredList<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableFilteredList<int>.Count)} property.");

			// Check that all items are sorted.
			for(int count=1; count<sortedList.Count; count++)
				Assert.IsTrue(sortedList[count] >= sortedList[count-1], $"Item at index {count} was not correctly sorted, its value was {sortedList[count]} but the previous index was {sortedList[count-1]}");
		}

		[TestMethod]
		public void ChangeFilter()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableSortedList<int>		sortedList			= new ConcurrentObservableSortedList<int>(collection, (int first, int second)=>first.CompareTo(second));
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			const int								iterations			= 10000;
			int										countChanged		= 0;
			int										clearCount			= 0;

			for(int count=0; count<iterations; count++)
				collection.Add(count);

			// Check that the collection contains as many sorted values as is expected.
			Assert.IsTrue(sortedList.Count == collection.Count());

			// Record every value provided by a collection changed event.
			sortedList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
					notificationValues.Add((int) e.NewItems.Count);
				else if(e.Action == NotifyCollectionChangedAction.Reset)
					clearCount++;
			};

			// Count number of times the Count property changes.
			sortedList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			// Change the definition of the comparison to sort by descending value.
			sortedList.Compare = (int first, int second)=>second.CompareTo(first);

			// Check that the collection contains as many sorted values as is expected.
			Assert.IsTrue(sortedList.Count == collection.Count());

			// Check that we received a single Add event, when the filter changed.
			Assert.IsTrue(notificationValues.Count == 1);
			
			// Check that the Add event contained as many elements as the source collection.
			Assert.IsTrue(notificationValues.First() == collection.Count());

			// Check that all items are sorted by descending value.
			for(int count=1; count<sortedList.Count; count++)
				Assert.IsTrue(sortedList[count] <= sortedList[count-1], $"Item at index {count} was not correctly sorted, its value was {sortedList[count]} but the previous index was {sortedList[count-1]}");
		}

		[TestMethod]
		public void ChangeSource()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableCollection<int>		newCollection		= new ConcurrentObservableCollection<int>();
			ConcurrentObservableSortedList<int>		sortedList			= new ConcurrentObservableSortedList<int>(collection, (int first, int second)=>first.CompareTo(second));
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			const int								iterations			= 10000;
			int										countChanged		= 0;
			int										clearCount			= 0;

			for(int count=0; count<iterations; count+=3)
				collection.Add(count);

			for(int count=0; count<iterations; count+=5)
				newCollection.Add(count);

			// Check that the collection contains as many filtered values as is expected.
			Assert.IsTrue(sortedList.Count == collection.Count());

			// Record every value provided by a collection changed event.
			sortedList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
					notificationValues.Add((int) e.NewItems.Count);
				else if(e.Action == NotifyCollectionChangedAction.Reset)
					clearCount++;
			};

			// Count number of times the Count property changes.
			sortedList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			// Change the source collection.
			sortedList.Source = newCollection;

			// Check that the collection contains as many filtered values as is expected.
			Assert.IsTrue(sortedList.Count == newCollection.Count());

			// Check that we received a single Add event, when the filter changed.
			Assert.IsTrue(notificationValues.Count == 1);
			
			// Check that the Add event contained as many elements as the source collection.
			Assert.IsTrue(notificationValues.First() == newCollection.Count());

			// Check that all items are sorted.
			for(int count=1; count<sortedList.Count; count++)
				Assert.IsTrue(sortedList[count] >= sortedList[count-1], $"Item at index {count} was not correctly sorted, its value was {sortedList[count]} but the previous index was {sortedList[count-1]}");
		}
	}
}
