using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Collections
{
	/// <summary>
	/// Filtered list, including those elements from another list, which meet the filter criteria and 
	/// provide change events when the list changes.
	/// 
	/// Listens to property changed events on each element in the list and re-evaluates the filter criteria on
	/// every property change, in order to determine if the changed element should be included.
	/// </summary>
	/// <typeparam name="T">Type of object to contain in the list.</typeparam>
	public class ConcurrentObservableFilteredList<T> :	ConcurrentObservableList<T>
	{
		#region Constructors
		/// <summary>
		/// Creates a new ConcurrentObservableFilteredList.
		/// </summary>
		/// <param name="source">Collection containing elements to filter.</param>
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
		public ConcurrentObservableFilteredList(ICollection<T> source, FilterDelegate filter, bool notifyUsingContext=false, bool notifyUsingAsync=false, SynchronizationContext context=null)
			:base(notifyUsingContext, notifyUsingAsync, context)
		{
			if(source == null)
				throw new ArgumentNullException(nameof(source));
			if(filter == null)
				throw new ArgumentNullException(nameof(filter));

			Filter = filter;
			Source = source;

			INotifyCollectionChanged notifyCollectionChanged = source as INotifyCollectionChanged;
			if(notifyCollectionChanged != null)
				notifyCollectionChanged.CollectionChanged += Source_CollectionChanged;
		}
		#endregion

		#region Types
		/// <summary>
		/// Determines if the specified element should be included in the list of filtered elements.
		/// </summary>
		/// <param name="element">Element to evaluate.</param>
		/// <returns>Returns true if the element should be included.</returns>
		public delegate bool FilterDelegate(T element);
		#endregion

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
		/// Filter used to evaluate whether an element should be included or in the list of filtered elements.
		/// </summary>
		public FilterDelegate Filter
		{
			get
			{
				return m_filter;
			}
			set
			{
				if(value == m_filter)
					return;

				if(value == null)
					throw new ArgumentNullException(nameof(Filter));

				lock(ListLockObject)
				{
					m_filter = value;

					// Refresh the list of filtered items, now that the filter has changed.
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
					// (Regardless of whether it is included in the filtered list).
					foreach(T item in value)
						ObserveElement(item);

					// Refresh the list of filtered items, now that the source collection has changed.
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
		/// Evaluates the changed elements, determining if the elements should be included in the filtered list.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">Information about the event.</param>
		protected virtual void Source_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				{
					foreach(T newItem in e.NewItems)
					{
						// Start listening to the element's properties changing 
						// (Regardless of whether it is included in the filtered list).
						ObserveElement(newItem);

						// Determine if the element should be added to the list of filtered elements.
						bool include = Filter(newItem);
						if(include)
							ProtectedAdd(newItem);
					}
				}
				break;

				case NotifyCollectionChangedAction.Remove:
				{
					foreach(T oldItem in e.OldItems)
					{
						// Stop listening to the element's properties changing.
						// (Regardless of whether it is included in the filtered list).
						StopObservingElement(oldItem);

						// Remove the element from the list of filtered elements (If it exists there).
						ProtectedRemove(oldItem);
					}
				}
				break;

				case NotifyCollectionChangedAction.Replace:
				{
					foreach(T oldItem in e.OldItems)
					{
						// Stop listening to the element's properties changing.
						// (Regardless of whether it is included in the filtered list).
						StopObservingElement(oldItem);

						// Remove the element from the list of filtered elements (If it exists there).
						ProtectedRemove(oldItem);
					}

					foreach(T newItem in e.NewItems)
					{
						// Start listening to the element's properties changing.
						// (Regardless of whether to it is included in the filtered list).
						ObserveElement(newItem);

						// Determine if the element should be added to the list of filtered elements.
						bool include = Filter(newItem);
						if(include)
							ProtectedAdd(newItem);
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

						// Clear the list of filtered elements.
						ProtectedClear();
					}
				}
				break;
			}
		}

		/// <summary>
		/// Evaluates the changed element, determining if the elements should be included in the filtered list.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A PropertyChangedEventArgs that contains the event data.</param>
		protected virtual void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			T element = (T) sender;

			// Determine if the changed element already exists.
			bool exists = Contains(element);

			// Determine if the element should exist in the list of filtered elements.
			bool include = Filter(element);
			if(include)
			{
				// If it should exist, but doesn't, add it now.
				if(!exists)
					ProtectedAdd(element);
			}
			else
			{
				// If it shouldn't exist, but does, remove it now.
				if(exists)
					ProtectedRemove(element);
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Refresh the list of filtered items.
		/// </summary>
		public virtual void Refresh()
		{
			// Clear the list of filtered elements.
			ProtectedClear();

			if(Source == null || Filter == null)
				return;

			foreach(T item in Source)
			{
				// Determine if the element should be added to the list of filtered elements.
				bool include = Filter(item);
				if(include)
					ProtectedAdd(item);
			}
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
		#endregion

		#region Fields
		protected const string ReadOnlyExceptionMessage = "The collection is read-only.";

		/// <summary>
		/// Backing field for the Filter property.
		/// </summary>
		private FilterDelegate m_filter;

		/// <summary>
		/// Backing field for the Source property.
		/// </summary>
		private ICollection<T> m_source;
		#endregion
	}
}
