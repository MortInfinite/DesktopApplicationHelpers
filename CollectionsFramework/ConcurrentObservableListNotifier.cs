using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Collections
{
	/// <summary>
	/// Listens to events raised by INotifyCollectionChanged and INotifyPropertyChanged, from a source collection,
	/// and then raises matching events using a specified synchronization context and optionally raises the events
	/// using asynchronous events.
	/// </summary>
	/// <typeparam name="T">Type of object to contain in the list.</typeparam>
	public class ConcurrentObservableListNotifier<T> :	IDisposable,
														IList<T>,
														IList,
														ICollection<T>, 
														IEnumerable<T>, 
														IEnumerable, 
														ICollection, 
														IReadOnlyCollection<T>, 
														INotifyCollectionChanged, 
														INotifyPropertyChanged,
														INotifyCollectionPropertyChanged
	{
		#region Constructors
		/// <summary>
		/// Creates a new ConcurrentObservableCollection.
		/// </summary>
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
		public ConcurrentObservableListNotifier(ICollection<T> source, bool notifyUsingContext=false, bool notifyUsingAsync=false, SynchronizationContext context=null)
		{
			if(notifyUsingContext)
			{
				Context = context;

				// If no synchronization context is specified.
				if(Context == null)
				{
					// If the current thread doesn't have a SynchronizationContext yet, create one now.
					if(SynchronizationContext.Current == null)
						SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());

					// Remember the SynchronizationContext to use.
					Context = SynchronizationContext.Current;
				}
				NotifyUsingAsync = notifyUsingAsync;
			}
			else if(notifyUsingAsync)
				throw new ArgumentException($"Can't set {nameof(notifyUsingAsync)} to true, when {nameof(notifyUsingContext)} is false.", nameof(notifyUsingAsync));

			Source = source;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Dispose of the object and its unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose pattern implementation.
		/// </summary>
		/// <param name="disposing">True if disposing, false if finalizing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			if(disposing)
			{
				lock(ListLockObject)
				lock(ObservedElementsLockObject)
				{
					// Unsubscribe from previous property changed event subscription.
					INotifyPropertyChanged notifyPropertyChanged = m_source as INotifyPropertyChanged;
					if(notifyPropertyChanged != null)
						notifyPropertyChanged.PropertyChanged -= Source_PropertyChanged;

					// Unsubscribe from previous collection changed event subscription.
					INotifyCollectionChanged notifyCollectionChanged = m_source as INotifyCollectionChanged;
					if(notifyCollectionChanged != null)
						notifyCollectionChanged.CollectionChanged -= Source_CollectionChanged;

					// Unsubscribe from observed elements.
					foreach(T oldItem in ObservedElements)
					{
						// Stop listening to the element's properties changing.
						notifyPropertyChanged = oldItem as INotifyPropertyChanged;
						if(notifyPropertyChanged != null)
							notifyPropertyChanged.PropertyChanged -= Element_PropertyChanged;
					}

					// Clear the list of observed elements.
					ObservedElements.Clear();
				}
			}

			Disposed = true;
		}

		/// <summary>
		/// Indicates if the object has been disposed.
		/// </summary>
		public bool Disposed
		{
			get;
			protected set;
		}
		#endregion

		#region INotifyPropertyChanged
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies subscribers that the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if(string.IsNullOrEmpty(propertyName))
                throw new ArgumentException($"The {nameof(propertyName)} argument wasn't specified.", nameof(propertyName));

			Action notifyDelegate = () =>
			{
	            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
			};

			Notify(notifyDelegate);
        }
		#endregion

		#region INotifyCollectionChanged implementation
		/// <summary>
		/// Occurs when the collection changes.
		/// </summary>
		public event NotifyCollectionChangedEventHandler CollectionChanged;

		/// <summary>
		/// Notifies subscribers that the collection changed.
		/// </summary>
		/// <param name="notifyCollectionChangedEventArgs">Information about the event.</param>
		protected virtual void NotifyCollectionChanged(NotifyCollectionChangedEventArgs notifyCollectionChangedEventArgs)
		{
			Action notifyDelegate = () =>
			{
				CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
			};

			Notify(notifyDelegate);
		}
		#endregion

		#region INotifyCollectionPropertyChanged
		/// <summary>
		/// Occurs when a property value changes, in an element contained in a collection.
		/// </summary>
		public event CollectionPropertyChangedDelegate CollectionPropertyChanged;

		/// <summary>
		/// Notifies subscribers that a property value has changed, in an element contained in the collection.
		/// </summary>
		/// <param name="notifyCollectionChangedEventArgs">Information about the event.</param>
		protected virtual void NotifyCollectionPropertyChanged(object element, string propertyName)
		{
			Action notifyDelegate = () =>
			{
				CollectionPropertyChanged?.Invoke(this, element, propertyName);
			};

			Notify(notifyDelegate);
		}

		#endregion

		#region IList<T> implementation
		/// <summary>
		/// Searches for the specified object and returns the zero-based index of the first
		///  occurrence within the entire List.
		/// </summary>
		/// <param name="item">The object to locate in the List. The value can be null for reference types.</param>
		/// <returns>
		/// The zero-based index of the first occurrence of item within the entire List, if found; otherwise, –1.
		/// </returns>
		public virtual int IndexOf(T item)
		{
			lock(ListLockObject)
			{
				IList<T> list = Source as IList<T>;
				if(list == null)
					throw new NotSupportedException("The Source collection doesn't support IList<T>.");

				return list.IndexOf(item);
			}
		}

		/// <summary>
		/// Inserts an element into the List at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which item should be inserted.</param>
		/// <param name="item">The object to insert. The value can be null for reference types.</param>
		/// <exception cref="ArgumentOutOfRangeException">Index is less than 0.-or-index is greater than Count.</exception>
		public virtual void Insert(int index, T item)
		{
			lock(ListLockObject)
			{
				IList<T> list = Source as IList<T>;
				if(list == null)
					throw new NotSupportedException("The Source collection doesn't support IList<T>.");

				list.Insert(index, item);
			}
		}

		/// <summary>
		/// Removes the element at the specified index of the List.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">Index is less than 0 or index is equal to or greater than Count.</exception>
		public virtual void RemoveAt(int index)
		{
			lock(ListLockObject)
			{
				IList<T> list = Source as IList<T>;
				if(list == null)
					throw new NotSupportedException("The Source collection doesn't support IList<T>.");

				list.RemoveAt(index);
			}
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <returns>The element at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Index is less than 0 or index is equal to or greater than Count.</exception>
		public virtual T this[int index]
		{
			get
			{
				lock(ListLockObject)
				{
					IList<T> list = Source as IList<T>;
					if(list == null)
						throw new NotSupportedException("The Source collection doesn't support IList<T>.");

					return list[index];
				}
			}
			set
			{
				lock(ListLockObject)
				{
					IList<T> list = Source as IList<T>;
					if(list == null)
						throw new NotSupportedException("The Source collection doesn't support IList<T>.");

					list[index] = value;
				}
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
		public virtual int Add(object value)
		{
			lock(ListLockObject)
			{
				IList list = Source as IList;
				if(list == null)
					throw new NotSupportedException("The Source collection doesn't support IList.");

				return list.Add(value);
			}
		}

		/// <summary>
		/// Determines whether the collection contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the collection.</param>
		/// <returns>
		/// true if item is found in the collection; otherwise, false.
		/// </returns>
		public virtual bool Contains(object value)
		{
			lock(ListLockObject)
			{
				IList list = Source as IList;
				if(list == null)
					throw new NotSupportedException("The Source collection doesn't support IList.");

				return list.Contains(value);
			}
		}

		/// <summary>
		/// Determines the index of a specific item in the list.
		/// </summary>
		/// <param name="value">The object to locate in the list.</param>
		/// <returns>The index of value if found in the list; otherwise, -1.</returns>
		public virtual int IndexOf(object value)
		{
			lock(ListLockObject)
			{
				IList list = Source as IList;
				if(list == null)
					throw new NotSupportedException("The Source collection doesn't support IList.");

				return list.IndexOf(value);
			}
		}

		/// <summary>
		/// Inserts an item to the list at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index at which value should be inserted.</param>
		/// <param name="value">The object to insert into the list.</param>
		/// <exception cref="ArgumentOutOfRangeException">The index is not a valid index in the list.</exception>
		/// <exception cref="NotSupportedException">The IList is read-only or The list has a fixed size.</exception>
		/// <exception cref="NullReferenceException">The value is null reference in the list.</exception>
		public virtual void Insert(int index, object value)
		{
			lock(ListLockObject)
			{
				IList list = Source as IList;
				if(list == null)
					throw new NotSupportedException("The Source collection doesn't support IList.");

				list.Insert(index, value);
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the collection.
		/// </summary>
		/// <param name="value">The object to remove from the collection.</param>
		/// <returns>
		/// true if item was successfully removed from the collection; otherwise, false. 
		/// This method also returns false if item is not found in the original collection.
		/// </returns>
		public virtual void Remove(object value)
		{
			lock(ListLockObject)
			{
				IList list = Source as IList;
				if(list == null)
					throw new NotSupportedException("The Source collection doesn't support IList.");

				list.Remove(value);
			}
		}

		/// <summary>
		/// Gets a value indicating whether the list has a fixed size.
		/// </summary>
		public virtual bool IsFixedSize
		{
			get
			{
				lock(ListLockObject)
				{
					IList list = Source as IList;
					if(list == null)
						throw new NotSupportedException("The Source collection doesn't support IList.");

					return list.IsFixedSize;
				}
			}
		}

		/// <summary>
		/// Gets or sets the element at the specified index.
		/// </summary>
		/// <param name="index">The zero-based index of the element to get or set.</param>
		/// <returns>The element at the specified index.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Index is less than 0 or index is equal to or greater than Count.</exception>
		object IList.this[int index]
		{
			get
			{
				lock(ListLockObject)
				{
					IList list = Source as IList;
					if(list == null)
						throw new NotSupportedException("The Source collection doesn't support IList.");

					return list[index];
				}
			}
			set
			{
				lock(ListLockObject)
				{
					IList list = Source as IList;
					if(list == null)
						throw new NotSupportedException("The Source collection doesn't support IList.");

					list[index] = value;
				}
			}
		}
		#endregion

		#region ICollection
		/// <summary>
		/// Gets an object that can be used to synchronize access to the collection.
		/// </summary>
		public virtual object SyncRoot
		{
			get
			{
				lock(ListLockObject)
				{
					ICollection collection = Source as ICollection;
					if(collection == null)
						throw new NotSupportedException("The Source collection doesn't support ICollection.");

					return collection.SyncRoot;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether access to the collection is synchronized (thread safe).
		/// </summary>
		public virtual bool IsSynchronized
		{
			get
			{
				lock(ListLockObject)
				{
					ICollection collection = Source as ICollection;
					if(collection == null)
						throw new NotSupportedException("The Source collection doesn't support ICollection.");

					return collection.IsSynchronized;
				}
			}
		}

		/// <summary>
		/// Copies the elements of the collection to an Array, starting at a particular Array index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional Array that is the destination of the elements copied from the collection. 
		/// The Array must have zero-based indexing.
		/// </param>
		/// <param name="index">The zero-based index in array at which copying begins.</param>
		public virtual void CopyTo(Array array, int index)
		{
			lock(ListLockObject)
			{
				lock(ListLockObject)
				{
					ICollection collection = Source as ICollection;
					if(collection == null)
						throw new NotSupportedException("The Source collection doesn't support ICollection.");

					collection.CopyTo(array, index);
				}
			}
		}
		#endregion

		#region ICollection<T> implementation
		/// <summary>
		/// Gets the number of elements contained in the collection.
		/// </summary>
		public virtual int Count
		{
			get
			{
				lock(ListLockObject)
				{
					ICollection<T> collection = Source as ICollection<T>;
					if(collection == null)
						throw new NotSupportedException("The Source collection doesn't support ICollection<T>.");

					return collection.Count;
				}
			}
		}

		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// </summary>
		public virtual bool IsReadOnly
		{
			get
			{
				lock(ListLockObject)
				{
					ICollection<T> collection = Source as ICollection<T>;
					if(collection == null)
						throw new NotSupportedException("The Source collection doesn't support ICollection<T>.");

					return collection.IsReadOnly;
				}
			}
		}

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="item">The object to add to the collection.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		public virtual void Add(T item)
		{
			lock(ListLockObject)
			{
				ICollection<T> collection = Source as ICollection<T>;
				if(collection == null)
					throw new NotSupportedException("The Source collection doesn't support ICollection<T>.");

				collection.Add(item);
			}
		}

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		public virtual void Clear()
		{
			lock(ListLockObject)
			{
				ICollection<T> collection = Source as ICollection<T>;
				if(collection == null)
					throw new NotSupportedException("The Source collection doesn't support ICollection<T>.");

				collection.Clear();
			}
		}

		/// <summary>
		/// Determines whether the collection contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the collection.</param>
		/// <returns>
		/// true if item is found in the collection; otherwise, false.
		/// </returns>
		public virtual bool Contains(T item)
		{
			lock(ListLockObject)
			{
				ICollection<T> collection = Source as ICollection<T>;
				if(collection == null)
					throw new NotSupportedException("The Source collection doesn't support ICollection<T>.");

				return collection.Contains(item);
			}
		}

		/// <summary>
		/// Copies the elements of the collection to an Array,
		/// starting at a particular Array index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional Array that is the destination of the elements copied
		/// from collection. The Array must have zero-based indexing.
		/// </param>
		/// <param name="index">
		/// The zero-based index in array at which copying begins.
		/// </param>
		/// <exception cref="ArgumentNullException">array is null.</exception>
		/// <exception cref="ArgumentOutOfRangeException">index is less than zero.</exception>
		/// <exception cref="ArgumentException">
		/// array is multidimensional.-or- The number of elements in the source collection
		/// is greater than the available space from index to the end of the destination
		/// array.-or-The type of the source collection cannot be cast
		/// automatically to the type of the destination array.
		/// </exception>
		public virtual void CopyTo(T[] array, int arrayIndex)
		{
			lock(ListLockObject)
			{
				ICollection<T> collection = Source as ICollection<T>;
				if(collection == null)
					throw new NotSupportedException("The Source collection doesn't support ICollection<T>.");

				collection.CopyTo(array, arrayIndex);
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the collection.
		/// </summary>
		/// <param name="item">The object to remove from the collection.</param>
		/// <returns>
		/// true if item was successfully removed from the collection; otherwise, false. 
		/// This method also returns false if item is not found in the original collection.
		/// </returns>
		public virtual bool Remove(T item)
		{
			lock(ListLockObject)
			{
				ICollection<T> collection = Source as ICollection<T>;
				if(collection == null)
					throw new NotSupportedException("The Source collection doesn't support ICollection<T>.");

				return collection.Remove(item);
			}
		}
		#endregion

		#region IEnumerable<T> implementation
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			lock(ListLockObject)
			{
				IEnumerable<T> enumerable = Source as IEnumerable<T>;
				if(enumerable == null)
					throw new NotSupportedException("The Source collection doesn't support IEnumerable<T>.");

				return enumerable.GetEnumerator();
			}
		}
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An enumerator object that can be used to iterate through the collection.
		/// </returns>
		public virtual IEnumerator GetEnumerator()
		{
			lock(ListLockObject)
			{
				IEnumerable enumerable = Source as IEnumerable;
				if(enumerable == null)
					throw new NotSupportedException("The Source collection doesn't support IEnumerable.");

				return enumerable.GetEnumerator();
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// Collection containing elements to filter.
		/// </summary>
		public ICollection<T> Source
		{
			get
			{
				return m_source;
			}
			protected set
			{
				if(value == m_source)
					return;

				if(value == null)
					throw new ArgumentNullException(nameof(Source));

				lock(ListLockObject)
				lock(ObservedElementsLockObject)
				{
					// Unsubscribe from previous property changed event subscription.
					INotifyPropertyChanged notifyPropertyChanged = m_source as INotifyPropertyChanged;
					if(notifyPropertyChanged != null)
						notifyPropertyChanged.PropertyChanged -= Source_PropertyChanged;

					// Unsubscribe from previous collection changed event subscription.
					INotifyCollectionChanged notifyCollectionChanged = m_source as INotifyCollectionChanged;
					if(notifyCollectionChanged != null)
						notifyCollectionChanged.CollectionChanged -= Source_CollectionChanged;

					m_source = value;

					// Subscribe to property changed event subscription.
					notifyPropertyChanged = m_source as INotifyPropertyChanged;
					if(notifyPropertyChanged != null)
						notifyPropertyChanged.PropertyChanged += Source_PropertyChanged;

					// Subscribe to collection changed event subscription.
					notifyCollectionChanged = m_source as INotifyCollectionChanged;
					if(notifyCollectionChanged != null)
						notifyCollectionChanged.CollectionChanged += Source_CollectionChanged;

					// Unsubscribe from observed elements.
					foreach(T oldItem in ObservedElements)
					{
						// Stop listening to the element's properties changing.
						notifyPropertyChanged = oldItem as INotifyPropertyChanged;
						if(notifyPropertyChanged != null)
							notifyPropertyChanged.PropertyChanged -= Element_PropertyChanged;
					}

					// Clear the list of observed elements.
					ObservedElements.Clear();

					// Start listening to the element's properties changing 
					// (Regardless of whether it is included in the filtered list).
					foreach(T item in value)
						ObserveElement(item);
				}

				// Notify subscribers that the collection has been replaced.
				NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
			}
		}

		/// <summary>
		/// Indicates if events should be raised asynchronously.
		/// </summary>
		public virtual bool NotifyUsingAsync
		{
			get;
			protected set;
		}

		/// <summary>
		/// SynchronizationContext used to raise events.
		/// 
		/// If this is null, events will be raised on the thread that modified the collection.
		/// </summary>
		public virtual SynchronizationContext Context
		{
			get;
			protected set;
		}

		/// <summary>
		/// Object used to lock the List for use in a single thread.
		/// </summary>
		protected virtual object ListLockObject
		{
			get;
		} = new object();

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

		#region Methods
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
		/// Calls the notifyDelegate using the method specified in the constructor of this class.
		/// </summary>
		/// <param name="notifyDelegate">
		/// Delegate to invoke. The delegate must raise the event.
		/// </param>
		/// <remarks>
		/// This method is only intended to be called by one of the other Notify methods.
		/// </remarks>
		protected virtual void Notify(Action notifyDelegate)
		{
			if(notifyDelegate == null)
				return;

			// If events must be raised using the synchronization context.
			if(Context != null)
			{
				if(NotifyUsingAsync)
					Context.Post((unused)=>notifyDelegate(), null);
				else
				{
					// If we are called on the same synchronization context as events must be raised on,
					// there is no need to use the Context to send the event.
					if(SynchronizationContext.Current == Context)
						notifyDelegate();
					else
						Context.Send((unused)=>notifyDelegate(), null);
				}
			}
			else
			{
				notifyDelegate();
			}
		}
		#endregion

		#region Event handlers
		/// <summary>
		/// Calls the notification event.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">Information about the event.</param>
		protected virtual void Source_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			NotifyPropertyChanged(e.PropertyName);
		}

		/// <summary>
		/// Calls the notification event.
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
						ObserveElement(newItem);
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
					}

					foreach(T newItem in e.NewItems)
					{
						// Start listening to the element's properties changing.
						// (Regardless of whether to it is included in the filtered list).
						ObserveElement(newItem);
					}
				}
				break;

				case NotifyCollectionChangedAction.Move:
				{
				}
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
					}
				}
				break;
			}

			// Notify subscribers that the collection has changed.
			NotifyCollectionChanged(e);
		}

		/// <summary>
		/// Evaluates the changed element, determining if the elements should be included in the filtered list.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A PropertyChangedEventArgs that contains the event data.</param>
		protected virtual void Element_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			// Notify subscribers that a property value has changed on an element in the collection.
			NotifyCollectionPropertyChanged(sender, e.PropertyName);
		}
		#endregion

		#region Fields
		/// <summary>
		/// Backing field for the Source property.
		/// </summary>
		private ICollection<T> m_source;
		#endregion
	}
}
