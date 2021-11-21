using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using Collections;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;
using System.Collections.Specialized;
using System.Threading;
using System.ComponentModel;
using System;
using System.Runtime.CompilerServices;
using System.Diagnostics;

namespace CollectionTest
{
	[TestClass]
	public class CollectionPropertyChangedListenerTest
	{
		[DebuggerDisplay("{First}")]
		public class DummyClass	:INotifyPropertyChanged
		{
			public DummyClass()
			{
			}

			public DummyClass(int first, int second)
			{
				First	= first;
				Second	= second;
			}

			#region INotifyPropertyChanged
			/// <summary>
			/// Occurs when a property value changes.
			/// </summary>
			public event PropertyChangedEventHandler PropertyChanged;

			/// <summary>
			/// Notifies subscribers that the property changed.
			/// </summary>
			/// <param name="propertyName"></param>
			protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName="")
			{
				if(string.IsNullOrEmpty(propertyName))
					throw new ArgumentException($"The {nameof(propertyName)} argument wasn't specified.", nameof(propertyName));

				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			}
			#endregion

			#region Properties
			/// <summary>
			/// First property
			/// </summary>
			public int First
			{
				get
				{
					return m_first;
				}
				set
				{
					// Don't set the property to its current value.
					if(value == m_first)
						return;

					m_first = value;

					// Notify subscribers that the property changed.
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(First)));
				}
			}

			/// <summary>
			/// Second property.
			/// </summary>
			public int Second
			{
				get
				{
					return m_second;
				}
				set
				{
					// Don't set the property to its current value.
					if(value == m_second)
						return;

					m_second = value;

					// Notify subscribers that the property changed.
					PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Second)));
				}
			}
			#endregion

			#region Methods
			public override bool Equals(object obj)
			{
				return Equals(obj as DummyClass);
			}

			public virtual bool Equals(DummyClass other)
			{
				if(other == null)
					return false;

				if(other.First != First)
					return false;

				if(other.Second != Second)
					return false;

				return true;
			}

			public override int GetHashCode()
			{
				int hashCode = 43270662;

				hashCode = hashCode * -1521134295 + First.GetHashCode();
				hashCode = hashCode * -1521134295 + Second.GetHashCode();

				return hashCode;
			}
			#endregion

			#region Fields
			/// <summary>
			/// Backing field for the First property.
			/// </summary>
			private int m_first;

			/// <summary>
			/// Backing field for the Second property.
			/// </summary>
			private int m_second;
			#endregion
		}

		/// <summary>
		/// Tests that:
		/// CollectionChanged event is raised, every time an item is added to the source list.
		/// PropertyChanged event is raised, every time an item is added to the source list.
		/// CollectionPropertyChanged event is raised, every time a property is modified in the source list.
		/// Multiple threads can add to the list, at the same time.
		/// Multiple threads can modify properties in the list, at the same time.
		/// </summary>
		[TestMethod]
		public async Task AddToSourceList()
		{
			const int										iterations				= 10000;
			ConcurrentObservableList<DummyClass>			list					= new ConcurrentObservableList<DummyClass>();
			CollectionPropertyChangedListener<DummyClass>	notifierList			= new CollectionPropertyChangedListener<DummyClass>(list);
			ConcurrentBag<int>								notificationValues		= new ConcurrentBag<int>();
			int												countChanged			= 0;
			int												firstPropertyChanged	= 0;

			// Record every value provided by a collection changed event.
			notifierList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				notificationValues.Add(((DummyClass) e.NewItems[0]).Second);
			};

			// Count number of times the Count property changes.
			notifierList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
			{
				if(e.PropertyName == nameof(ConcurrentObservableListNotifier<int>.Count))
					Interlocked.Increment(ref countChanged);
			};

			// Count number of times the First property changes, in an element.
			notifierList.CollectionPropertyChanged += (object sender, object element, string propertyName) =>
			{
				DummyClass dummyObject = (DummyClass) element;

				if(propertyName == nameof(DummyClass.First))
				{
					Interlocked.Increment(ref firstPropertyChanged);

					// Check that the First property now has the same value as the Second property.
					Assert.AreEqual(dummyObject.First, dummyObject.Second);
				}
			};

			List<int> two = new List<int>();
			for(int count=1; count<iterations; count+=2)
				two.Add(count);

			List<int> three = new List<int>();
			for(int count=1; count<iterations; count+=3)
				three.Add(count);

			List<int> five = new List<int>();
			for(int count=1; count<iterations; count+=5)
				five.Add(count);

			// Add integers divisible by two, to the source collection.
			Task twoTask = Task.Run(async ()=>
			{
				Parallel.ForEach(two, (value)=>
				{
					DummyClass newValue = new DummyClass(0, value);

					list.Add(newValue);
					newValue.First	= value;
				});

				await Task.Delay(1);
			});

			// Add integers divisible by three, to the source collection.
			Task threeTask = Task.Run(async ()=>
			{
				Parallel.ForEach(three, (value)=>
				{
					DummyClass newValue = new DummyClass(0, value);

					list.Add(newValue);
					newValue.First	= value;
				});
				await Task.Delay(1);
			});

			// Add integers divisible by five, to the source collection.
			Task fiveTask = Task.Run(async ()=>
			{
				Parallel.ForEach(five, (value)=>
				{
					DummyClass newValue = new DummyClass(0, value);

					list.Add(newValue);
					newValue.First	= value;
				});
				await Task.Delay(1);
			});

			// Wait for all numbers to be added.
			await Task.WhenAll(twoTask, threeTask, fiveTask);

			// Check that we received an event for each value that was added.
			Assert.IsTrue(notificationValues.Count == two.Count+three.Count+five.Count);

			// Check that an event was received for each value that was added.
			List<int> notificationValuesArray = notificationValues.ToList();

			// Check that the notifier collection property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(two.Count+three.Count+five.Count, firstPropertyChanged, $"The {nameof(CollectionPropertyChangedListener<DummyClass>.CollectionPropertyChanged)} event wasn't raised the expected number of times for the {nameof(DummyClass.First)} property.");

			// Remove event subscriptions
			notifierList.Dispose();
		}

		/// <summary>
		/// Tests that:
		/// CollectionChanged event is raised, every time an item is removed from the source list.
		/// PropertyChanged event is raised, every time an item is added or removed from the source list.
		/// Multiple threads can add to the list, at the same time.
		/// Multiple threads can remove from the list, at the same time.
		/// Elements that were removed, are no longer in the list.
		/// Elements that were not removed, are still in the list.
		/// </summary>
		[TestMethod]
		public void RemoveFromSourceList()
		{
			const int										iterations					= 10000;
			ConcurrentObservableList<DummyClass>			list						= new ConcurrentObservableList<DummyClass>();
			CollectionPropertyChangedListener<DummyClass>	notifierList				= new CollectionPropertyChangedListener<DummyClass>(list);
			ConcurrentBag<DummyClass>						notificationValuesRemoved	= new ConcurrentBag<DummyClass>();
			int												countChanged				= 0;

			// Record every value provided by a collection changed event.
			notifierList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				switch(e.Action)
				{
					case NotifyCollectionChangedAction.Remove:
						notificationValuesRemoved.Add((DummyClass) e.OldItems[0]);
					break;
				}
			};

			// Count number of times the Count property changes.
			notifierList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
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
			Parallel.ForEach(two, (value)=>
			{
				DummyClass newValue = new DummyClass(0, value);

				list.Add(newValue);
				newValue.First	= value;
			});

			// Remove integers divisible by three.
			Parallel.ForEach(three, (value)=>
			{
				DummyClass dummyValue = new DummyClass(value, value);

				list.Remove(dummyValue);
			});

			// Check that all values from the two collection exists, unless it also exists in the three collection.
			foreach(int value in two)
			{
				DummyClass dummyValue = new DummyClass(value, value);

				if(three.Contains(value))
					Assert.IsFalse(list.Contains(dummyValue), $"The value {value} wasn't removed from the collection.");
				else
					Assert.IsTrue(list.Contains(dummyValue), $"The value {value} wasn't added to the collection or was removed although it shouldn't be.");
			}

			int numberOfThreesRemoved = 0;

			// Check that we received Remove events for every item removed.
			foreach(int value in three)
			{
				DummyClass dummyValue = new DummyClass(value, value);

				if(two.Contains(value))
				{
					Assert.IsTrue(notificationValuesRemoved.Contains(dummyValue), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableCollection<int>.CollectionChanged)} event.");
					numberOfThreesRemoved++;
				}
				else
					Assert.IsFalse(notificationValuesRemoved.Contains(dummyValue), $"The value {value} shouldn't have been raised by the {nameof(ConcurrentObservableCollection<int>.CollectionChanged)} event.");
			}

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(two.Count+numberOfThreesRemoved, countChanged, $"The {nameof(ConcurrentObservableCollection<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableCollection<int>.Count)} property.");

			// Remove event subscriptions
			notifierList.Dispose();
		}

		/// <summary>
		/// Tests that:
		/// CollectionChanged event is raised, every time an item is removed from the notifier list.
		/// PropertyChanged event is raised, every time an item is added or removed from the notifier list.
		/// Multiple threads can add to the list, at the same time.
		/// Multiple threads can remove from the list, at the same time.
		/// Elements that were removed, are no longer in the list.
		/// Elements that were not removed, are still in the list.
		/// </summary>
		[TestMethod]
		public void RemoveFromNotifierList()
		{
			const int										iterations					= 10000;
			ConcurrentObservableList<DummyClass>			list						= new ConcurrentObservableList<DummyClass>();
			ConcurrentObservableListNotifier<DummyClass>	notifierList				= new ConcurrentObservableListNotifier<DummyClass>(list);
			ConcurrentBag<DummyClass>						notificationValuesRemoved	= new ConcurrentBag<DummyClass>();
			int												countChanged				= 0;

			// Record every value provided by a collection changed event.
			notifierList.CollectionChanged += (object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e) =>
			{
				switch(e.Action)
				{
					case NotifyCollectionChangedAction.Remove:
						notificationValuesRemoved.Add((DummyClass) e.OldItems[0]);
					break;
				}
			};

			// Count number of times the Count property changes.
			notifierList.PropertyChanged += (object sender, System.ComponentModel.PropertyChangedEventArgs e) =>
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
			Parallel.ForEach(two, (value)=>
			{
				DummyClass newValue = new DummyClass(0, value);

				notifierList.Add(newValue);
				newValue.First	= value;
			});

			// Remove integers divisible by three.
			Parallel.ForEach(three, (value)=>
			{
				DummyClass dummyValue = new DummyClass(value, value);

				notifierList.Remove(dummyValue);
			});

			// Check that all values from the two collection exists, unless it also exists in the three collection.
			foreach(int value in two)
			{
				DummyClass dummyValue = new DummyClass(value, value);

				if(three.Contains(value))
					Assert.IsFalse(list.Contains(dummyValue), $"The value {value} wasn't removed from the collection.");
				else
					Assert.IsTrue(list.Contains(dummyValue), $"The value {value} wasn't added to the collection or was removed although it shouldn't be.");
			}

			int numberOfThreesRemoved = 0;

			// Check that we received Remove events for every item removed.
			foreach(int value in three)
			{
				DummyClass dummyValue = new DummyClass(value, value);

				if(two.Contains(value))
				{
					Assert.IsTrue(notificationValuesRemoved.Contains(dummyValue), $"The value {value} wasn't raised by the {nameof(ConcurrentObservableCollection<int>.CollectionChanged)} event.");
					numberOfThreesRemoved++;
				}
				else
					Assert.IsFalse(notificationValuesRemoved.Contains(dummyValue), $"The value {value} shouldn't have been raised by the {nameof(ConcurrentObservableCollection<int>.CollectionChanged)} event.");
			}

			// Check that the property notify changed event was called the correct number of times, for the Count property.
			Assert.AreEqual(two.Count+numberOfThreesRemoved, countChanged, $"The {nameof(ConcurrentObservableCollection<int>.PropertyChanged)} event wasn't raised the expected number of times for the {nameof(ConcurrentObservableCollection<int>.Count)} property.");

			// Remove event subscriptions
			notifierList.Dispose();
		}
	}
}
