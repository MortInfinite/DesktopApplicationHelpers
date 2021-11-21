using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Collections
{
	/// <summary>
	/// Thread safe read only wrapper, which provides events when the list changes.
	/// 
	/// Trying to modify the contained list will throw a NotSupportedException.
	/// 
	/// Although the list itself is read only, the objects it contain can still be modified.
	/// </summary>
	/// <typeparam name="T">Type of object to contain in the list.</typeparam>
	public class ConcurrentObservableReadOnlyList<T>	:ConcurrentObservableList<T>
	{
		#region Constructors
		/// <summary>
		/// Creates a new ConcurrentObservableReadOnlyList.
		/// </summary>
		/// <param name="source">List to provide read only access to.</param>
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
		public ConcurrentObservableReadOnlyList(IList<T> source, bool notifyUsingContext=false, bool notifyUsingAsync=false, SynchronizationContext context=null)
			:base(notifyUsingContext, notifyUsingAsync, context)
		{
			List = source;

			INotifyCollectionChanged notifyCollectionChanged = source as INotifyCollectionChanged;
			if(notifyCollectionChanged != null)
				notifyCollectionChanged.CollectionChanged += List_CollectionChanged;

			INotifyPropertyChanged notifyPropertyChanged = source as INotifyPropertyChanged;
			if(notifyPropertyChanged != null)
				notifyPropertyChanged.PropertyChanged += List_PropertyChanged;
		}
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

		#region Event handlers
		/// <summary>
		/// Notifies subscribers that the collection changed.
		/// </summary>
		/// <param name="sender">The object that raised the event.</param>
		/// <param name="e">Information about the event.</param>
		protected virtual void List_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			Action notifyDelegate = () =>
			{
				NotifyCollectionChanged(e);
			};

			NotifyCollectionChanged(notifyDelegate);
		}

		/// <summary>
		/// Notifies subscribers that the collection changed.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">A PropertyChangedEventArgs that contains the event data.</param>
		protected virtual void List_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Action notifyDelegate = () =>
			{
				NotifyPropertyChanged(e.PropertyName);
			};

			NotifyCollectionChanged(notifyDelegate);
		}
		#endregion

		#region Fields
		protected const string ReadOnlyExceptionMessage = "The collection is read-only.";
		#endregion
	}
}
