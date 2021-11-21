using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Collections
{
	/// <summary>
	/// Observes items in a primary list and uses those items to generate values in a secondary list.
	/// 
	/// This is used to maintain a list of values, calculated based on the contents of another list.
	/// </summary>
	/// <typeparam name="T1">Type of item in the primary list.</typeparam>
	/// <typeparam name="T2">Type of item in the secondary list.</typeparam>
	/// <remarks>
	/// This class does NOT listen to changes in the properties contained in the primary list, it only listens to changes to the list itself.
	/// </remarks>
	public class DualList<T1, T2> :ICollection<DualItem<T1, T2>>
	{
		/// <summary>
		/// Create a new dual list.
		/// </summary>
		/// <param name="primaryList">List to observe.</param>
		/// <param name="secondaryList">List to create items in, based on the items from the <paramref name="primaryList"/>.</param>
		/// <param name="createSecondary">Delegate used to create an item in the secondary list.</param>
		/// <param name="updateSecondary">Delegate used to update an existing item in the secondary list, when an item from the primary list is replaced.</param>
		public DualList(IList<T1> primaryList, IList<T2> secondaryList, CreateDelegate createSecondary=null, UpdateDelegate updateSecondary=null)
		{
			if(primaryList == null)
				throw new ArgumentNullException(nameof(primaryList));
			if(secondaryList == null)
				throw new ArgumentNullException(nameof(secondaryList));

			m_primaryList   = primaryList;
			m_secondaryList	= secondaryList;
			CreateSecondary	= createSecondary;
			UpdateSecondary	= updateSecondary;

			INotifyCollectionChanged primaryCollectionChanged = primaryList as INotifyCollectionChanged;
			if(primaryCollectionChanged != null)
				primaryCollectionChanged.CollectionChanged  += PrimaryCollectionChanged_CollectionChanged;

			INotifyCollectionChanged secondaryCollectionChanged = secondaryList as INotifyCollectionChanged;
			if(secondaryCollectionChanged != null)
				secondaryCollectionChanged.CollectionChanged  += SecondaryCollectionChanged_CollectionChanged;

			FillDualItemList();
		}

		#region ICollection<T> implementation
		/// <summary>
		/// Gets the number of elements contained in the System.Collections.ICollection.
		/// </summary>
		public int Count
		{
			get
			{
				return m_primaryList.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the list is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return m_primaryList.IsReadOnly;
			}
		}

		/// <summary>
		/// Adds an item to the list.
		/// </summary>
		/// <param name="item">The object to add to the list.</param>
		/// <exception cref="NotSupportedException">The list is read-only.</exception>
		public void Add(DualItem<T1, T2> item)
		{
			try
			{
				IgnoreCollectionChanging = true;

				m_primaryList.Add(item.Primary);
				m_secondaryList.Add(item.Secondary);
			}
			finally
			{
				IgnoreCollectionChanging = false;
			}
		}

		/// <summary>
		/// Removes all items from the System.Collections.Generic.ICollection.
		/// </summary>
		/// <exception cref="NotSupportedException">The System.Collections.Generic.ICollection`1 is read-only.</exception>
		public void Clear()
		{
			try
			{
				IgnoreCollectionChanging = true;

				m_primaryList.Clear();
				m_secondaryList.Clear();
			}
			finally
			{
				IgnoreCollectionChanging = false;
			}
		}

		/// <summary>
		/// Determines whether the System.Collections.Generic.ICollection`1 contains a specific value.
		/// </summary>
		/// <param name="item">The object to locate in the System.Collections.Generic.ICollection`1.</param>
		/// <returns>
		/// true if item is found in the System.Collections.Generic.ICollection`1; otherwise, false.
		/// </returns>
		public bool Contains(DualItem<T1, T2> item)
		{
			foreach(DualItem<T1, T2> current in this)
			{
				bool equals = EqualityComparer<T1>.Default.Equals(current.Primary, item.Primary);
				if(!equals)
					continue;

				equals = EqualityComparer<T2>.Default.Equals(current.Secondary, item.Secondary);
				if(!equals)
					continue;

				return true;
			}

			return false;
		}

		/// <summary>
		/// Copies the elements of the System.Collections.ICollection to an System.Array,
		/// starting at a particular System.Array index.
		/// </summary>
		/// <param name="array">
		/// The one-dimensional System.Array that is the destination of the elements copied
		/// from System.Collections.ICollection. The System.Array must have zero-based indexing.
		/// </param>
		/// <param name="index">
		/// The zero-based index in array at which copying begins.
		/// </param>
		/// <exception cref="System.ArgumentNullException">array is null.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">index is less than zero.</exception>
		/// <exception cref="System.ArgumentException">
		/// array is multidimensional.-or- The number of elements in the source System.Collections.ICollection
		/// is greater than the available space from index to the end of the destination
		/// array.-or-The type of the source System.Collections.ICollection cannot be cast
		/// automatically to the type of the destination array.
		/// </exception>
		public void CopyTo(DualItem<T1, T2>[] array, int arrayIndex)
		{
			int count = 0;

			try
			{
				IgnoreCollectionChanging = true;

				foreach(object currentItem in this)
					array.SetValue(currentItem, arrayIndex+count++);
			}
			finally
			{
				IgnoreCollectionChanging = false;
			}
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the list.
		/// </summary>
		/// <param name="item">The object to remove from the list.</param>
		/// <returns>
		/// true if item was successfully removed from the list; otherwise, false. 
		/// This method also returns false if item is not found in the original list.
		/// </returns>
		public bool Remove(DualItem<T1, T2> item)
		{
			// To remove the matching pair of items, the primary list needs the RemoveAt method,
			// which isn't supported by ICollection<T>.
			IList<T1> primaryList = m_primaryList as IList<T1>;
			if(primaryList == null)
				throw new NotSupportedException("The primary list doesn't support removing the item.");

			try
			{
				IgnoreCollectionChanging = true;

				for(int index=0; index<primaryList.Count; index++)
				{
					bool equals = EqualityComparer<T1>.Default.Equals(primaryList[index], item.Primary);
					if(!equals)
						continue;

					equals = EqualityComparer<T2>.Default.Equals(m_secondaryList[index], item.Secondary);
					if(!equals)
						continue;

					primaryList.RemoveAt(index);
					m_secondaryList.RemoveAt(index);

					return true;
				}
			}
			finally
			{
				IgnoreCollectionChanging = false;
			}

			return false;
		}
		#endregion

		#region IEnumerable<T> implementation
		/// <summary>
		/// Returns an enumerator that iterates through the list.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the list.</returns>
		IEnumerator<DualItem<T1, T2>> IEnumerable<DualItem<T1, T2>>.GetEnumerator()
		{
			IEnumerator<DualItem<T1, T2>> enumerator = m_dualItemList.GetEnumerator();
			return enumerator;
		}
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Returns an enumerator that iterates through a list.
		/// </summary>
		/// <returns>
		/// An System.Collections.IEnumerator object that can be used to iterate through the list.
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			IEnumerator enumerator = m_dualItemList.GetEnumerator();
			return enumerator;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Delegate used to create an object in the secondary list,
		/// in response to an object in the primary list being added.
		/// </summary>
		/// <param name="index">Index being created or modified.</param>
		/// <param name="primaryValue">Object being created or modified in the other list.</param>
		/// <returns>Object to add to the secondary list.</returns>
		public delegate T2 CreateDelegate(int index, T1 primaryValue);

		/// <summary>
		/// Delegate used to update an object in the secondary list, 
		/// in response to an object in the primary list being modified.
		/// </summary>
		/// <param name="index">Index being modified.</param>
		/// <param name="primaryValue">Value being set on the primary list.</param>
		/// <param name="currentValue">Current value of the object in the secondary list.</param>
		/// <returns>Value to set on the object in the secondary list.</returns>
		public delegate T2 UpdateDelegate(int index, T1 primaryValue, T2 currentValue);

		/// <summary>
		/// Implementation to use to create an object in the secondary list, when 
		/// the primary list grows.
		/// </summary>
		public CreateDelegate CreateSecondary
		{
			get
			{
				if(m_createSecondary == null)
					return CreateSecondaryDefault;

				return m_createSecondary;
			}
			set
			{
				m_createSecondary = value;

				// Now that it's possible to create secondary elements, fill out the secondary list
				// with elements, such that it contains as many elements as the primary list.
				SynchronizeCollectionSizes();
			}
		}

		/// <summary>
		/// Implementation to use to modify an object in the secondary list, when 
		/// the primary list is modified.
		/// </summary>
		public UpdateDelegate UpdateSecondary
		{
			get
			{
				if(m_updateSecondary == null)
					return UpdateSecondaryDefault;

				return m_updateSecondary;
			}
			set
			{
				m_updateSecondary = value;
			}
		}

		/// <summary>
		/// Indicates if the secondary list is being modified in response to the 
		/// event handler of the primary list.
		/// </summary>
		private bool SecondaryCollectionLocked
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates that the primary and secondary lists are both about to change and
		/// to not execute the event handler of the primary list.
		/// </summary>
		private bool IgnoreCollectionChanging
		{
			get;
			set;
		}
		#endregion

		#region Event handlers
		/// <summary>
		/// Updates the secondary list in response to the primary list being updated.
		/// </summary>
		private void PrimaryCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(IgnoreCollectionChanging)
				return;

			try
			{
				SecondaryCollectionLocked = true;

				switch(e.Action)
				{
					case NotifyCollectionChangedAction.Add:
					{
						// If it wasn't possible to update the secondary list to match the primary list.
						if(CreateSecondary == null)
							return;

						for(int count = 0; count<e.NewItems.Count; count++)
						{
							object matchingObject = CreateSecondary(e.NewStartingIndex+count, (T1) e.NewItems[count]);
							m_secondaryList.Insert(e.NewStartingIndex+count, (T2) matchingObject);

							DualItem<T1, T2> dualItem = new DualItem<T1, T2>(m_primaryList, m_secondaryList, count);
							m_dualItemList.Insert(e.NewStartingIndex+count, dualItem);
						}
					}
					break;

					case NotifyCollectionChangedAction.Move:
					{
						List<T2> removedObjects = new List<T2>(e.OldItems.Count);

						for(int count = 0; count<e.OldItems.Count; count++)
						{
							removedObjects.Add(m_secondaryList[e.OldStartingIndex]);

							m_secondaryList.RemoveAt(e.OldStartingIndex);
						}

						for(int count = 0; count<e.NewItems.Count; count++)
						{
							m_secondaryList.Insert(e.NewStartingIndex+count, removedObjects[count]);
						}
					}
					break;

					case NotifyCollectionChangedAction.Remove:
					{
						for(int count=0; count<e.OldItems.Count; count++)
						{
							m_secondaryList.RemoveAt(e.OldStartingIndex+count);
							m_dualItemList.RemoveAt(e.OldStartingIndex+count);
						}
					}
					break;

					case NotifyCollectionChangedAction.Replace:
					{
						// If it wasn't possible to update the secondary list to match the primary list.
						if(UpdateSecondary == null)
							return;

						for(int count = 0; count<e.NewItems.Count; count++)
						{
							object matchingObject = UpdateSecondary(e.NewStartingIndex+count, (T1) e.NewItems[count], m_secondaryList[count]);
							m_secondaryList[e.NewStartingIndex+count] = (T2) matchingObject;
						}
					}
					break;

					case NotifyCollectionChangedAction.Reset:
					{
						m_secondaryList.Clear();
						m_dualItemList.Clear();
					}
					break;
				}
			}
			finally
			{
				SecondaryCollectionLocked = false;
			}
		}

		/// <summary>
		/// Updates the primary list in response to the secondary list being updated.
		/// </summary>
		private void SecondaryCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(IgnoreCollectionChanging)
				return;

			if(SecondaryCollectionLocked)
				return;

			switch(e.Action)
			{
				case NotifyCollectionChangedAction.Add:
				case NotifyCollectionChangedAction.Remove:
				case NotifyCollectionChangedAction.Move:
				case NotifyCollectionChangedAction.Reset:
					throw new InvalidOperationException("Modifying the SecondaryCollection is not permitted, while it's part of the DualCollection.");
			}
		}

		#endregion

		#region Methods
		/// <summary>
		/// Creates a null object reference.
		/// </summary>
		/// <param name="index">Index at which to create the object.</param>
		/// <param name="primaryValue">Object for which to create a matching object.</param>
		/// <returns>Null reference.</returns>
		private T2 CreateSecondaryDefault(int index, T1 primaryValue)
		{
			return default(T2);
		}

		/// <summary>
		/// Creates a null object reference.
		/// </summary>
		/// <param name="index">Index being modified.</param>
		/// <param name="primaryValue">Value being set on the primary list.</param>
		/// <param name="currentValue">Current value of the object in the secondary list.</param>
		/// <returns>Value to set on the object in the secondary list.</returns>
		private T2 UpdateSecondaryDefault(int index, T1 primaryValue, T2 currentValue)
		{
			return default(T2);
		}

		/// <summary>
		/// Fill out the secondary list with values, until the secondary list contains as many values
		/// as the primary list.
		/// 
		/// If CreateSecondary is null, this method does nothing.
		/// </summary>
		private void SynchronizeCollectionSizes()
		{
			if(CreateSecondary == null)
				return;

			if(m_primaryList.Count < m_secondaryList.Count)
			{
				// Remove extra elements from the secondary list, until the secondary list
				// only contains as many elements as the primary list.
				while(m_secondaryList.Count > m_primaryList.Count)
					m_secondaryList.RemoveAt(m_primaryList.Count);
			}

			if(m_primaryList.Count > m_secondaryList.Count)
			{
				// Add missing elements to the secondary list, until the secondary list
				// contains as many elements as the primary list.
				int index = m_secondaryList.Count;
				IEnumerable<T1> primaryEnumerator = m_primaryList.Skip(index);
				foreach(T1 primaryValue in primaryEnumerator)
				{
					T2 secondaryValue = CreateSecondary(index, primaryValue);
					m_secondaryList.Add(secondaryValue);
				}
			}
		}

		/// <summary>
		/// Fill the dual item list with values from the primary and secondary lists.
		/// </summary>
		private void FillDualItemList()
		{
			for(int count=m_dualItemList.Count; count<m_primaryList.Count; count++)
			{
				DualItem<T1, T2> dualItem = new DualItem<T1, T2>(m_primaryList, m_secondaryList, count);
				m_dualItemList.Add(dualItem);
			}
		}
		#endregion

		#region Fields
		IList<T1>				m_primaryList;
		IList<T2>				m_secondaryList;
		IList<DualItem<T1, T2>>	m_dualItemList			= new List<DualItem<T1, T2>>();
		CreateDelegate			m_createSecondary;
		UpdateDelegate			m_updateSecondary;
		#endregion
	}
}
