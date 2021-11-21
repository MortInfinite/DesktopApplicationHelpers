using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Collections
{
	/// <summary>
	/// Delegate used evaluate which of two elements are of greater value.
	/// </summary>
	/// <param name="element">Element to evaluate.</param>
	/// <returns>
	/// Returns -1 if the first element is less than the second element.
	/// Returns 0 if the first element and second element are of the same value.
	/// Returns 1 if the first element is greater than the second element.
	/// </returns>
	public delegate int CompareDelegate<T>(T first, T second);

	/// <summary>
	/// </summary>
	/// <typeparam name="T">Type of object to contain in the list.</typeparam>
	public class ConcurrentObservableSortedList<T> :	ConcurrentObservableList<T>
	{
		#region Constructors
		/// <summary>
		/// Creates a new ConcurrentObservableFilteredList.
		/// </summary>
		/// <param name="source">Collection containing elements to sort.</param>
		/// <param name="compare">Implementation used to determine which element is of greater value.</param>
		/// <param name="notifyUsingContext">When true, raises events using the specified context.</param>
		/// <param name="notifyUsingAsync">
		/// When true, raises events asynchronously. 
		/// Please note that when setting this argument to true, notifyUsingContext must also be set to true.
		/// </param>
		/// <param name="context">
		/// Synchronization context used to raise events. 
		/// 
		/// When null is specified, the current thread's synchronization context is used.
		/// If the current thread doesn't have a synchronization context, one is created.
		/// </param>
		public ConcurrentObservableSortedList(ICollection<T> source, CompareDelegate<T> compare, bool notifyUsingContext=false, bool notifyUsingAsync=false, SynchronizationContext context=null)
			:base(notifyUsingContext, notifyUsingAsync, context)
		{
			if(source == null)
				throw new ArgumentNullException(nameof(source));
			if(compare == null)
				throw new ArgumentNullException(nameof(compare));

			Compare = compare;
			Source = source;

			INotifyCollectionChanged notifyCollectionChanged = source as INotifyCollectionChanged;
			if(notifyCollectionChanged != null)
				notifyCollectionChanged.CollectionChanged += Source_CollectionChanged;
		}
		#endregion
/*
		#region Types
		/// <summary>
		/// Delegate used evaluate which of two elements are of greater value.
		/// </summary>
		/// <param name="element">Element to evaluate.</param>
		/// <returns>
		/// Returns -1 if the first element is less than the second element.
		/// Returns 0 if the first element and second element are of the same value.
		/// Returns 1 if the first element is greater than the second element.
		/// </returns>
		public delegate int CompareDelegate(T first, T second);
		#endregion
*/
		#region ICollection<T> implementation
		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// </summary>
		public override bool IsReadOnly
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="item">The object to add to the collection.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		public override void Add(T item)
		{
			throw new NotSupportedException(ReadOnlyExceptionMessage);
		}

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		public override void Clear()
		{
			throw new NotSupportedException(ReadOnlyExceptionMessage);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the collection.
		/// </summary>
		/// <param name="item">The object to remove from the collection.</param>
		/// <returns>
		/// true if item was successfully removed from the collection; otherwise, false. 
		/// This method also returns false if item is not found in the original collection.
		/// </returns>
		public override bool Remove(T item)
		{
			throw new NotSupportedException(ReadOnlyExceptionMessage);
		}
		#endregion

		#region IList<T> implementation
		/// <summary>
		/// Inserts an element into the List at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		/// <exception cref="ArgumentOutOfRangeException">Index is less than 0.-or-index is greater than Count.</exception>
		public override void Insert(int index, T item)
		{
			throw new NotSupportedException(ReadOnlyExceptionMessage);
		}

		/// <summary>
		/// Removes the element at the specified index of the List.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">Index is less than 0 or index is equal to or greater than Count.</exception>
		public override void RemoveAt(int index)
		{
			throw new NotSupportedException(ReadOnlyExceptionMessage);
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <returns>The element at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Index is less than 0 or index is equal to or greater than Count.</exception>
		public override T this[int index]
		{
			set
			{
				throw new NotSupportedException(ReadOnlyExceptionMessage);
			}
		}
		#endregion

		#region IList implementation
		/// <summary>
		/// Adds an item to the List.
		/// </summary>
		/// <param name="value">The object to add to the List.</param>
		/// <returns>
		/// The position into which the new element was inserted, or -1 to indicate that
		/// the item was not inserted into the collection.
		/// </returns>
		public override int Add(object value)
		{
			throw new NotSupportedException(ReadOnlyExceptionMessage);
		}

		/// <summary>
		/// Inserts an item to the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="value">The object to insert into the list.</param>
		/// <exception cref="ArgumentOutOfRangeException">The index is not a valid index in the list.</exception>
		/// <exception cref="NotSupportedException">The IList is read-only or The list has a fixed size.</exception>
		/// <exception cref="NullReferenceException">The value is null reference in the list.</exception>
		public override void Insert(int index, object value)
		{
			throw new NotSupportedException(ReadOnlyExceptionMessage);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the collection.
		/// </summary>
		/// <param name="value">The object to remove from the collection.</param>
		/// <returns>
		/// true if item was successfully removed from the collection; otherwise, false. 
		/// This method also returns false if item is not found in the original collection.
		/// </returns>
		public override void Remove(object value)
		{
			throw new NotSupportedException(ReadOnlyExceptionMessage);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Comparison implementation used evaluate which of two elements are of greater value.
		/// </summary>
		public CompareDelegate<T> Compare
		{
			get
			{
				return m_compare;
			}
			set
			{
				if(value == m_compare)
					return;

				if(value == null)
					throw new ArgumentNullException(nameof(Compare));

				lock(ListLockObject)
				{
					m_compare = value;

					// Refresh the list of sorted items, now that the sort implementation has changed.
					Refresh();
				}
			}
		}

		/// <summary>
		/// Collection containing elements to filter.
		/// </summary>
		public ICollection<T> Source
		{
			get
			{
				return m_source;
			}
			set
			{
				if(value == m_source)
					return;

				if(value == null)
					throw new ArgumentNullException(nameof(Source));

				lock(ListLockObject)
				lock(ObservedElementsLockObject)
				{
					m_source = value;

					foreach(T oldItem in ObservedElements)
					{
						// Stop listening to the element's properties changing.
						INotifyPropertyChanged notifyPropertyChanged = oldItem as INotifyPropertyChanged;
						if(notifyPropertyChanged != null)
							notifyPropertyChanged.PropertyChanged -= Element_PropertyChanged;
					}

					// Clear the list of observed elements.
					ObservedElements.Clear();

					// Start listening to the element's properties changing 
					// (Regardless of whether it is included in the sorted list).
					foreach(T item in value)
						ObserveElement(item);

					// Refresh the list of sorted items, now that the source collection has changed.
					Refresh();
				}
			}
		}

		/// <summary>
		/// List containing all observed elements.
		/// </summary>
		protected virtual IList<T> ObservedElements
		{
			get;
			set;
		} = new List<T>();

		/// <summary>
		/// Object used to lock the ObservedElements for use in a single thread.
		/// </summary>
		protected virtual object ObservedElementsLockObject
		{
			get;
		} = new object();
		#endregion

		#region Event handlers
		/// <summary>
		/// Evaluates the changed elements, determining if the elements should be included in the sorted list.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">Information about the event.</param>
		protected virtual void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			lock(ListLockObject)
			{
				switch(e.Action)
				{
					case NotifyCollectionChangedAction.Add:
					{
						foreach(T newItem in e.NewItems)
						{
							// Start listening to the element's properties changing 
							// (Regardless of whether it is included in the sorted list).
							ObserveElement(newItem);

							// Determine where in the list to insert the new item.
							int insertIndex = GetSortedIndex(newItem);
							ProtectedInsert(insertIndex, newItem);
						}
					}
					break;

					case NotifyCollectionChangedAction.Remove:
					{
						foreach(T oldItem in e.OldItems)
						{
							// Stop listening to the element's properties changing.
							// (Regardless of whether it is included in the sorted list).
							StopObservingElement(oldItem);

							// Remove the element from the list of sorted elements (If it exists there).
							ProtectedRemove(oldItem);
						}
					}
					break;

					case NotifyCollectionChangedAction.Replace:
					{
						foreach(T oldItem in e.OldItems)
						{
							// Stop listening to the element's properties changing.
							// (Regardless of whether it is included in the sorted list).
							StopObservingElement(oldItem);

							// Remove the element from the list of sorted elements (If it exists there).
							ProtectedRemove(oldItem);
						}

						foreach(T newItem in e.NewItems)
						{
							// Start listening to the element's properties changing.
							// (Regardless of whether to it is included in the sorted list).
							ObserveElement(newItem);

							// Determine where in the list to insert the new item.
							int insertIndex = GetSortedIndex(newItem);
							ProtectedInsert(insertIndex, newItem);
						}
					}
					break;

					case NotifyCollectionChangedAction.Move:
						// Don't do anything to a moving element.
					break;

					case NotifyCollectionChangedAction.Reset:
					{
						lock(ObservedElementsLockObject)
						{
							foreach(T oldItem in ObservedElements)
							{
								// Stop listening to the element's properties changing.
								INotifyPropertyChanged notifyPropertyChanged = oldItem as INotifyPropertyChanged;
								if(notifyPropertyChanged != null)
									notifyPropertyChanged.PropertyChanged -= Element_PropertyChanged;
							}

							// Clear the list of observed elements.
							ObservedElements.Clear();

							// Clear the list of sorted elements.
							ProtectedClear();
						}
					}
					break;
				}
			}
		}

		/// <summary>
		/// Evaluates the changed element, determining its new index in the sorted list.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A PropertyChangedEventArgs that contains the event data.</param>
		protected virtual void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			T element = (T) sender;

			ProtectedSort(element);
		}
		#endregion

		#region Methods
		/// <summary>
		/// Refresh the list of sorted items.
		/// </summary>
		public virtual void Refresh()
		{
			lock(ListLockObject)
			{
				// Clear the list of sorted elements.
				ProtectedClear();

				if(Source == null || Compare == null)
					return;

				foreach(T newItem in Source)
					List.Add(newItem);

				List.MergeSort(0, List.Count-1, Compare);

				/*
				foreach(T newItem in Source)
				{
					// Determine where in the list to insert the new item.
					int insertIndex = GetSortedIndex(newItem);
					ProtectedInsert(insertIndex, newItem);
				}
				*/
			}

			NotifyItemsAdded(this, 0);
		}

		/// <summary>
		/// Add a property changed subscription to the specified element.
		/// </summary>
		/// <param name="element">Element to observe.</param>
		protected virtual void ObserveElement(T element)
		{
			lock(ObservedElementsLockObject)
			{
				INotifyPropertyChanged notifyPropertyChanged = element as INotifyPropertyChanged;
				if(notifyPropertyChanged != null)
					notifyPropertyChanged.PropertyChanged += Element_PropertyChanged;

				ObservedElements.Add(element);
			}
		}

		/// <summary>
		/// Remove a property changed subscription from the specified element.
		/// </summary>
		/// <param name="element">Element to stop observing.</param>
		protected virtual void StopObservingElement(T element)
		{
			lock(ObservedElementsLockObject)
			{
				INotifyPropertyChanged notifyPropertyChanged = element as INotifyPropertyChanged;
				if(notifyPropertyChanged != null)
					notifyPropertyChanged.PropertyChanged -= Element_PropertyChanged;

				ObservedElements.Remove(element);
			}
		}

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="item">The object to add to the collection.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		protected virtual void ProtectedAdd(T item)
		{
			lock(ListLockObject)
			{
				List.Add(item);
			}

			NotifyItemAdded(item, List.Count-1);
		}

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		protected virtual void ProtectedClear()
		{
			lock(ListLockObject)
			{
				List.Clear();
			}

			NotifyClear();
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the collection.
		/// </summary>
		/// <param name="item">The object to remove from the collection.</param>
		/// <returns>
		/// true if item was successfully removed from the collection; otherwise, false. 
		/// This method also returns false if item is not found in the original collection.
		/// </returns>
		protected virtual bool ProtectedRemove(T item)
		{
			int index;

			lock(ListLockObject)
			{
				// Get the index of the item to remove.
				index = List.IndexOf(item);
				if(index < 0)
					return false;

				List.RemoveAt(index);
			}

			NotifyItemRemoved(item, index);

			return true;
		}

		/// <summary>
		/// Inserts an item at the specified index of the list.
		/// </summary>
		/// <param name="index">Index at which to insert the item.</param>
		/// <param name="item">The object to add to the list.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		protected virtual void ProtectedInsert(int index, T item)
		{
			lock(ListLockObject)
			{
				List.Insert(index, item);
			}

			NotifyItemAdded(item, index);
		}

		/// <summary>
		/// Moves an item from and old position, in the list, to a new position, in the list.
		/// </summary>
		/// <param name="oldIndex">Index of the item to move.</param>
		/// <param name="newIndex">Destination index to which the item will be moved.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		protected virtual void ProtectedMove(T item, int oldIndex, int newIndex)
		{
			lock(ListLockObject)
			{
				// Remove the item from its current position.
				List.RemoveAt(oldIndex);

				// Insert the item at its new position.
				List.Insert(newIndex, item);
			}

			NotifyItemMoved(item, oldIndex, newIndex);
		}

		/// <summary>
		/// Sorts an item, by moving it from and old position, in the list, to a new position, in the list.
		/// </summary>
		/// <param name="item">Item to sort.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		protected virtual void ProtectedSort(T item)
		{
			int oldIndex;
			int newIndex;

			lock(ListLockObject)
			{
				// Determine if the current index of the changed element.
				oldIndex = List.IndexOf(item);
				if(oldIndex < 0)
					return;

				// Determine if the item needs to be sorted.
				if(!NeedsSort(oldIndex))
					return;

				// Determine if the element should exist in the list of sorted elements.
				newIndex  = GetSortedIndex(item);
				if(newIndex ==  oldIndex)
					return;

				// Remove the item from its current position.
				List.RemoveAt(oldIndex);

				// Insert the item at its new position.
				List.Insert(newIndex, item);
			}

			NotifyItemMoved(item, oldIndex, newIndex);
		}

		/// <summary>
		/// Determine if the item, at the specified index, needs to be sorted.
		/// </summary>
		/// <param name="index">Index of item to examine.</param>
		/// <returns>Returns true if the item at the specified index needs to be sorted.</returns>
		protected virtual bool NeedsSort(int index)
		{
			lock(ListLockObject)
			{
				T item = List[index];
			
				// If the item has a less value than the previous index.
				if(index-1 >= 0 && Compare(item, List[index-1]) < 0)
					return true;

				// If the item has a greater value than the next index.
				if(index+1 < List.Count && Compare(item, List[index+1]) > 0)
					return true;
			}

			return false;
		}

		/// <summary>
		/// Determines the index where to insert the specified item.
		/// </summary>
		/// <param name="item">Item to determine which insert to insert at.</param>
		/// <returns>Index at which the item should be inserted.</returns>
		protected virtual int GetSortedIndex(T item)
		{
			lock(ListLockObject)
			{
				int index = GetSortedInsertIndex(item, 0, List.Count-1);
				return index;
			}
		}

		/// <summary>
		/// Perform a binary search on the specified list, to determine the index at which to insert the <paramref name="value"/>.
		/// </summary>
		/// <param name="list">List to determine insert index of.</param>
		/// <param name="value">Value to search for the sorted insert index of.</param>
		/// <param name="startIndex">Index of first element, in the list, to search.</param>
		/// <param name="endIndex">Index of last element, in the list, to search.</param>
		/// <returns>Index of a result that matches the specified value.</returns>
		/// <remarks>Lock the <see cref="ListLockObject"/> before calling this method.</remarks>
		protected int GetSortedInsertIndex(T value, int startIndex, int endIndex)
		{
			if(List.Count == 0)
				return 0;
			if(startIndex > endIndex)
				return -1;

			int middleIndex = (endIndex-startIndex)/2+startIndex;
			
			int compareResult = Compare(value, List[middleIndex]);

			// Value is same value as value at middle index. New item must be inserted after the middle index.
			if(compareResult == 0)
				return middleIndex+1;

			// Value is less than value at middle index.
			if(compareResult < 0)
			{
				// Middle index is at the first item in the list. New item must be inserted before the first item.
				if(middleIndex == startIndex)
					return middleIndex;

				return GetSortedInsertIndex(value, startIndex, middleIndex-1);
			}
			
			// Value is greater than value at middle index.
			if(compareResult > 0)
			{
				// Middle index is at the last item in the list. New item must be inserted after the last item.
				if(middleIndex == endIndex)
					return middleIndex+1;
				
				return GetSortedInsertIndex(value, middleIndex+1, endIndex);
			}

			return -1;
		}
		#endregion

		#region Fields
		protected const string ReadOnlyExceptionMessage = "The collection is read-only.";

		/// <summary>
		/// Backing field for the <see cref="Compare"/> property.
		/// </summary>
		private CompareDelegate<T> m_compare;

		/// <summary>
		/// Backing field for the <see cref="Source"/> property.
		/// </summary>
		private ICollection<T> m_source;
		#endregion
	}
}
