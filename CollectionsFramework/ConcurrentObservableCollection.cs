using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Collections
{
	/// <summary>
	/// Thread safe collection, which provides events when the collection changes.
	/// </summary>
	/// <typeparam name="T">Type of object to contain in the collection.</typeparam>
	public class ConcurrentObservableCollection<T>: ICollection<T>, 
													IEnumerable<T>, 
													IEnumerable, 
													ICollection, 
													IReadOnlyCollection<T>, 
													INotifyCollectionChanged, 
													INotifyPropertyChanged
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
		public ConcurrentObservableCollection(bool notifyUsingContext=false, bool notifyUsingAsync=false, SynchronizationContext context=null)
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
        /// <param name="propertyName"></param>
        protected virtual void NotifyPropertyChanged(string propertyName)
        {
            if(string.IsNullOrEmpty(propertyName))
                throw new ArgumentException($"The {nameof(propertyName)} argument wasn't specified.", nameof(propertyName));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
		#endregion

		#region Notification methods
		/// <summary>
		/// Notifies subscribers that an item was added.
		/// </summary>
		/// <param name="item">Item that was added.</param>
		/// <param name="index">Index at which the item was added.</param>
		protected virtual void NotifyItemAdded(T item, int index)
		{
			Action notifyDelegate = () =>
			{
				NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item, index));
				NotifyPropertyChanged(nameof(Count));
			};

			NotifyCollectionChanged(notifyDelegate);
		}

		/// <summary>
		/// Notifies subscribers that a list of items was added.
		/// </summary>
		/// <param name="item">Items that were added.</param>
		/// <param name="startIndex">Index of the first item which was added.</param>
		protected virtual void NotifyItemsAdded(IList items, int startIndex)
		{
			Action notifyDelegate = () =>
			{
				NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, items, startIndex));
				NotifyPropertyChanged(nameof(Count));
			};

			NotifyCollectionChanged(notifyDelegate);
		}

		/// <summary>
		/// Notifies subscribers that the collection of items was cleared.
		/// </summary>
		protected virtual void NotifyClear()
		{
			Action notifyDelegate = () =>
			{
				NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
				NotifyPropertyChanged(nameof(Count));
			};

			NotifyCollectionChanged(notifyDelegate);
		}

		/// <summary>
		/// Notifies subscribers that an item was removed.
		/// </summary>
		/// <param name="item">Item that was removed.</param>
		/// <param name="index">Index of the item which was removed.</param>
		protected virtual void NotifyItemRemoved(T item, int index)
		{
			Action notifyDelegate = () =>
			{
				NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item, index));
				NotifyPropertyChanged(nameof(Count));
			};

			NotifyCollectionChanged(notifyDelegate);
		}

		/// <summary>
		/// Calls the notifyDelegate using the method specified in the constructor of this class.
		/// </summary>
		/// <param name="notifyDelegate">
		/// Delegate to invoke. 
		/// The delegate must raise the CollectionChanged event.
		/// </param>
		/// <remarks>
		/// This method is only intended to be called by one of the other Notify methods.
		/// </remarks>
		protected virtual void NotifyCollectionChanged(Action notifyDelegate)
		{
			// If nobody subscribed to change events.
			if(CollectionChanged == null && PropertyChanged == null)
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
			CollectionChanged?.Invoke(this, notifyCollectionChangedEventArgs);
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
					return ((ICollection) List).SyncRoot;
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
				return true;
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
				((ICollection) List).CopyTo(array, index);
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
					return List.Count;
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
				return false;
			}
		}

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="item">The object to add to the collection.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		public virtual void Add(T item)
		{
			int index;

			lock(ListLockObject)
			{
				List.Add(item);

				index = List.Count-1;
			}

			NotifyItemAdded(item, index);
		}

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		public virtual void Clear()
		{
			lock(ListLockObject)
			{
				List.Clear();
			}

			NotifyClear();
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
				return List.Contains(item);
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
				List.CopyTo(array, arrayIndex);
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

		#region IEnumerable<T> implementation
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator<T> IEnumerable<T>.GetEnumerator()
		{
			lock(ListLockObject)
			{
				IEnumerator<T> enumerator = List.GetEnumerator();
				return enumerator;
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
				IEnumerator enumerator = List.GetEnumerator();
				return enumerator;
			}
		}
		#endregion

		#region Properties
		/// <summary>
		/// List containing all of the elements in this collection.
		/// </summary>
		protected virtual IList<T> List
		{
			get;
			set;
		} = new List<T>();

		/// <summary>
		/// Object used to lock the List for use in a single thread.
		/// </summary>
		protected virtual object ListLockObject
		{
			get;
		} = new object();

		/// <summary>
		/// SynchronizationContext used to raise events.
		/// 
		/// If this is null, events will be raised on the thread that modified the collection.
		/// </summary>
		protected virtual SynchronizationContext Context
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates if events should be raised asynchronously.
		/// </summary>
		public virtual bool NotifyUsingAsync
		{
			get;
			protected set;
		}
		#endregion
	}
}
