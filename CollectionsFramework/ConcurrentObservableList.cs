﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Threading;

namespace Collections
{
	/// <summary>
	/// Thread safe list, which provides events when the list changes.
	/// </summary>
	/// <typeparam name="T">Type of object to contain in the list.</typeparam>
	public class ConcurrentObservableList<T> :	ConcurrentObservableCollection<T>, 
												IList<T>,
												IList
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
		public ConcurrentObservableList(bool notifyUsingContext=false, bool notifyUsingAsync=false, SynchronizationContext context=null)
			:base(notifyUsingContext, notifyUsingAsync, context)
		{
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
				return List.IndexOf(item);
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
				List.Insert(index, item);
			}

			NotifyItemAdded(item, index);
		}

		/// <summary>
		/// Removes the element at the specified index of the List.
		/// </summary>
		/// <param name="index">The zero-based index of the element to remove.</param>
		/// <exception cref="ArgumentOutOfRangeException">Index is less than 0 or index is equal to or greater than Count.</exception>
		public virtual void RemoveAt(int index)
		{
			T item;

			lock(ListLockObject)
			{
				// Get a reference to the removed item.
				item = List[index];

				List.RemoveAt(index);
			}

			NotifyItemRemoved(item, index);
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
					return List[index];
				}
			}
			set
			{
				T previousValue;

				lock(ListLockObject)
				{
					previousValue = List[index];

					List[index] = value;
				}

				NotifyItemReplaced(value, previousValue, index);
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
			int result;

			lock(ListLockObject)
			{
				result = ((IList) List).Add(value);
			}

			if(result >= 0)
				NotifyItemAdded((T) value, result);

			return result;
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
				return ((IList) List).Contains(value);
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
				return ((IList) List).IndexOf(value);
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
				((IList) List).Insert(index, value);
			}

			NotifyItemAdded((T) value, index);
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
			int index;

			lock(ListLockObject)
			{
				// Get the index of the item to remove.
				index = List.IndexOf((T) value);
				if(index < 0)
					return;

				((IList) List).RemoveAt(index);
			}

			NotifyItemRemoved((T) value, index);
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
					return ((IList) List).IsFixedSize;
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
				return this[index];
			}
			set
			{
				this[index] = (T) value;
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Moves the item at the specified index to a new location in the collection.
		/// </summary>
		/// <param name="oldIndex">The zero-based index specifying the location of the item to be moved.</param>
		/// <param name="newIndex">The zero-based index specifying the new location of the item.</param>
		/// <remarks>
		/// When the move command has completed, the item that was previously located at oldIndex
		/// can now be found at newIndex. Please note that the newIndex refers to the index after
		/// the item has been removed from the list.
		/// </remarks>
		public void Move(int oldIndex, int newIndex)
		{
			T item;

			lock(ListLockObject)
			{
				item = this[oldIndex];

				List.RemoveAt(oldIndex);
				List.Insert(newIndex, item);
			}

			NotifyItemMoved(item, oldIndex, newIndex);
		}
		#endregion

		#region Notification methods
		/// <summary>
		/// Notifies subscribers that an item was replaced.
		/// </summary>
		/// <param name="newItem">The new item that is replacing the original.</param>
		/// <param name="oldItem">The original item that is replaced.</param>
		/// <param name="index">Index at which the item was added.</param>
		protected virtual void NotifyItemReplaced(T newItem, T oldItem, int index)
		{
			Action notifyDelegate = () =>
			{
				NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Replace, newItem, oldItem, index));
			};

			NotifyCollectionChanged(notifyDelegate);
		}

		/// <summary>
		/// Notifies subscribers that an item was moved.
		/// </summary>
		/// <param name="item">The item that was moved.</param>
		/// <param name="oldItem">Index from which the item was removed.</param>
		/// <param name="newIndex">Index to which the item was moved.</param>
		protected virtual void NotifyItemMoved(T item, int oldIndex, int newIndex)
		{
			Action notifyDelegate = () =>
			{
				NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, item, newIndex, oldIndex));
			};

			NotifyCollectionChanged(notifyDelegate);
		}

		/// <summary>
		/// Notifies subscribers that a list of items were moved.
		/// </summary>
		/// <param name="item">Items that were moved.</param>
		/// <param name="newIndex">The new start index of the moved items.</param>
		/// <param name="index">The old start index of the moved items.</param>
		protected virtual void NotifyItemsMoved(IList items, int oldIndex, int newIndex)
		{
			Action notifyDelegate = () =>
			{
				NotifyCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Move, items, newIndex, oldIndex));
			};

			NotifyCollectionChanged(notifyDelegate);
		}
		#endregion
	}
}
