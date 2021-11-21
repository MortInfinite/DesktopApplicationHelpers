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
	public class ConcurrentObservableFilteredListTest
	{
		[TestMethod]
		public async Task Add()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableFilteredList<int>	filteredList		= new ConcurrentObservableFilteredList<int>(collection, (int element)=>element % 3 == 0);
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			int										countChanged		= 0;
			const int								iterations			= 10000;

			// Record every value provided by a collection changed event.
			filteredList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				notificationValues.Add((int) e.NewItems[0]);
			};

			// Count number of times the Count property changes.
			filteredList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			List<int> two			= new List<int>();
			List<int> filteredTwo	= new List<int>();
			for(int count=0; count<iterations; count+=2)
			{
				two.Add(count);

				if(count % 3 == 0)
					filteredTwo.Add(count);
			}

			List<int> three			= new List<int>();
			List<int> filteredThree = new List<int>();
			for(int count=0; count<iterations; count+=3)
			{
				three.Add(count);

				if(count % 3 == 0)
					filteredThree.Add(count);
			}

			List<int> five			= new List<int>();
			List<int> filteredFive	= new List<int>();
			for(int count=0; count<iterations; count+=5)
			{
				five.Add(count);

				if(count % 3 == 0)
					filteredFive.Add(count);
			}

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
			Assert.IsTrue(filteredList.Count == filteredTwo.Count+filteredThree.Count+filteredFive.Count);

			// Check that all added values are found in the collection.
			Parallel.ForEach(filteredTwo,	(value)=>Assert.IsTrue(filteredList.Contains(value), $"The value {value} wasn't added from the {nameof(filteredTwo)} list."));
			Parallel.ForEach(filteredThree,	(value)=>Assert.IsTrue(filteredList.Contains(value), $"The value {value} wasn't added from the {nameof(filteredThree)} list."));
			Parallel.ForEach(filteredFive,	(value)=>Assert.IsTrue(filteredList.Contains(value), $"The value {value} wasn't added from the {nameof(filteredFive)} list."));

			// Check that we received an event for each value that was added.
			Assert.IsTrue(notificationValues.Count == filteredTwo.Count+filteredThree.Count+filteredFive.Count);

			// Check that an event was received for each value that was added.
			List<int> notificationValuesArray = notificationValues.ToList();
			foreach(int value in filteredTwo)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in filteredThree)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in filteredFive)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(filteredTwo.Count+filteredThree.Count+filteredFive.Count, countChanged, $"The {nameof(ConcurrentObservableFilteredList<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableFilteredList<int>.Count)} property.");
		}

		[TestMethod]
		public async Task Remove()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableFilteredList<int>	filteredList		= new ConcurrentObservableFilteredList<int>(collection, (int element)=>element % 3 == 0);
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			int										countChanged		= 0;
			const int								iterations			= 10000;

			// Record every value provided by a collection changed event.
			filteredList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
					notificationValues.Add((int) e.NewItems[0]);
				else if(e.Action == NotifyCollectionChangedAction.Remove)
					notificationValues.Add((int) e.OldItems[0]);
			};

			// Count number of times the Count property changes.
			filteredList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			List<int> five			= new List<int>();
			List<int> filteredFive	= new List<int>();
			for(int count=0; count<iterations; count+=5)
			{
				five.Add(count);

				if(count % 3 == 0)
					filteredFive.Add(count);
			}

			List<int> six			= new List<int>();
			List<int> filteredSix	= new List<int>();
			for(int count=0; count<iterations; count+=6)
			{
				six.Add(count);

				if(count % 3 == 0)
					filteredSix.Add(count);
			}

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
			Assert.IsTrue(filteredList.Count == filteredFive.Count);

			// Check that all added values are found in the collection.
			Parallel.ForEach(filteredFive,	(value)=>Assert.IsTrue(filteredList.Contains(value), $"The value {value} wasn't added from the {nameof(filteredFive)} list."));

			// Check that we received an event for each value that was added.
			Assert.IsTrue(notificationValues.Count == filteredFive.Count+filteredSix.Count+filteredSix.Count);

			// Check that an event was received for each value that was added.
			List<int> notificationValuesArray = notificationValues.ToList();
			foreach(int value in filteredFive)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in filteredSix)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(filteredFive.Count+filteredSix.Count+filteredSix.Count, countChanged, $"The {nameof(ConcurrentObservableFilteredList<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableFilteredList<int>.Count)} property.");
		}

		[TestMethod]
		public async Task Clear()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableFilteredList<int>	filteredList		= new ConcurrentObservableFilteredList<int>(collection, (int element)=>element % 3 == 0);
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			int										clearCount			= 0;
			int										countChanged		= 0;
			const int								iterations			= 10000;

			// Record every value provided by a collection changed event.
			filteredList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
					notificationValues.Add((int) e.NewItems[0]);
				else if(e.Action == NotifyCollectionChangedAction.Reset)
					clearCount++;
			};

			// Count number of times the Count property changes.
			filteredList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			List<int> two			= new List<int>();
			List<int> filteredTwo	= new List<int>();
			for(int count=0; count<iterations; count+=2)
			{
				two.Add(count);

				if(count % 3 == 0)
					filteredTwo.Add(count);
			}

			List<int> three			= new List<int>();
			List<int> filteredThree = new List<int>();
			for(int count=0; count<iterations; count+=3)
			{
				three.Add(count);

				if(count % 3 == 0)
					filteredThree.Add(count);
			}

			List<int> five			= new List<int>();
			List<int> filteredFive	= new List<int>();
			for(int count=0; count<iterations; count+=5)
			{
				five.Add(count);

				if(count % 3 == 0)
					filteredFive.Add(count);
			}

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
			Assert.IsTrue(notificationValues.Count == filteredTwo.Count+filteredThree.Count);

			// Check that we received an event every time the collection was cleared.
			Assert.IsTrue(clearCount == five.Count);

			// Check that an event was received for each value that was added.
			List<int> notificationValuesArray = notificationValues.ToList();
			foreach(int value in filteredTwo)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");
			foreach(int value in filteredThree)
				Assert.IsTrue(notificationValuesArray.Contains(value), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableFilteredList<int>.CollectionChanged)} event.");

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.IsTrue(countChanged > filteredTwo.Count+filteredThree.Count, $"The {nameof(ConcurrentObservableFilteredList<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableFilteredList<int>.Count)} property.");
		}

		[TestMethod]
		public void ChangeFilter()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableFilteredList<int>	filteredList		= new ConcurrentObservableFilteredList<int>(collection, (int element)=>element % 3 == 0);
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			const int								iterations			= 10000;
			int										countChanged		= 0;
			int										clearCount			= 0;

			for(int count=0; count<iterations; count++)
				collection.Add(count);

			// Check that the collection contains as many filtered values as is expected.
			Assert.IsTrue(filteredList.Count == collection.Where((value) => value % 3 == 0).Count());

			// Record every value provided by a collection changed event.
			filteredList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
					notificationValues.Add((int) e.NewItems[0]);
				else if(e.Action == NotifyCollectionChangedAction.Reset)
					clearCount++;
			};

			// Count number of times the Count property changes.
			filteredList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			// Change the definition of the filter.
			filteredList.Filter = (int element)=>element % 7 == 0;

			// Check that the collection contains as many filtered values as is expected.
			Assert.IsTrue(filteredList.Count == collection.Where((value) => value % 7 == 0).Count());

			// Check that we received an event for each value that was added, when the filter changed.
			Assert.IsTrue(notificationValues.Count == collection.Where((value) => value % 7 == 0).Count());

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(collection.Where((value) => value % 7 == 0).Count()+clearCount, countChanged, $"The {nameof(ConcurrentObservableFilteredList<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableCollection<int>.Count)} property.");
		}

		[TestMethod]
		public void ChangeSource()
		{
			ConcurrentObservableCollection<int>		collection			= new ConcurrentObservableCollection<int>();
			ConcurrentObservableCollection<int>		newCollection		= new ConcurrentObservableCollection<int>();
			ConcurrentObservableFilteredList<int>	filteredList		= new ConcurrentObservableFilteredList<int>(collection, (int element)=>element % 2 == 0);
			ConcurrentBag<int>						notificationValues	= new ConcurrentBag<int>();
			const int								iterations			= 10000;
			int										countChanged		= 0;
			int										clearCount			= 0;

			for(int count=0; count<iterations; count+=3)
				collection.Add(count);

			for(int count=0; count<iterations; count+=5)
				newCollection.Add(count);

			// Check that the collection contains as many filtered values as is expected.
			Assert.IsTrue(filteredList.Count == collection.Where((value) => value % 2 == 0).Count());

			// Record every value provided by a collection changed event.
			filteredList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				if(e.Action == NotifyCollectionChangedAction.Add)
					notificationValues.Add((int) e.NewItems[0]);
				else if(e.Action == NotifyCollectionChangedAction.Reset)
					clearCount++;
			};

			// Count number of times the Count property changes.
			filteredList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableFilteredList<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			// Change the source collection.
			filteredList.Source = newCollection;

			// Check that the collection contains as many filtered values as is expected.
			Assert.IsTrue(filteredList.Count == newCollection.Where((value) => value % 2 == 0).Count());

			// Check that we received an event for each value that was added, when the filter changed.
			Assert.IsTrue(notificationValues.Count == newCollection.Where((value) => value % 2 == 0).Count());

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(newCollection.Where((value) => value % 2 == 0).Count()+clearCount, countChanged, $"The {nameof(ConcurrentObservableFilteredList<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableCollection<int>.Count)} property.");
		}
	}
}
