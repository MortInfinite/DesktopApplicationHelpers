using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collections
{
	public class DualCollection<T1, T2> :ICollection<Tuple<T1, T2>>
	{
		public DualCollection(ICollection<T1> primaryCollection, IList<T2> secondaryCollection, CreateDelegate createSecondary=null, UpdateDelegate updateSecondary=null)
		{
			if(primaryCollection == null)
				throw new ArgumentNullException(nameof(primaryCollection));
			if(secondaryCollection == null)
				throw new ArgumentNullException(nameof(secondaryCollection));
			//if(primaryCollection.Count != secondaryCollection.Count)
			//	throw new ArgumentException("The primaryCollection and secondaryCollection do not contain the same number of elements.");

			m_primaryCollection     = primaryCollection;
			m_secondaryCollection   = secondaryCollection;
			CreateSecondary			= createSecondary;
			UpdateSecondary			= updateSecondary;

			INotifyCollectionChanged primaryCollectionChanged = primaryCollection as INotifyCollectionChanged;
			if(primaryCollectionChanged != null)
				primaryCollectionChanged.CollectionChanged  += PrimaryCollectionChanged_CollectionChanged;

			INotifyCollectionChanged secondaryCollectionChanged = secondaryCollection as INotifyCollectionChanged;
			if(secondaryCollectionChanged != null)
				secondaryCollectionChanged.CollectionChanged  += SecondaryCollectionChanged_CollectionChanged;
		}

		#region ICollection<T> implementation
		/// <summary>
		/// Gets the number of elements contained in the System.Collections.ICollection.
		/// </summary>
		public int Count
		{
			get
			{
				return m_primaryCollection.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// </summary>
		public bool IsReadOnly
		{
			get
			{
				return m_primaryCollection.IsReadOnly;
			}
		}

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="item">The object to add to the collection.</param>
		/// <exception cref="NotSupportedException">The collection is read-only.</exception>
		public void Add(Tuple<T1, T2> item)
		{
			try
			{
				IgnoreCollectionChanging = true;

				m_primaryCollection.Add(item.Item1);
				m_secondaryCollection.Add(item.Item2);
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

				m_primaryCollection.Clear();
				m_secondaryCollection.Clear();
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
		public bool Contains(Tuple<T1, T2> item)
		{
			foreach(Tuple<T1, T2> current in this)
			{
				bool equals = EqualityComparer<T1>.Default.Equals(current.Item1, item.Item1);
				if(!equals)
					continue;

				equals = EqualityComparer<T2>.Default.Equals(current.Item2, item.Item2);
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
		public void CopyTo(Tuple<T1, T2>[] array, int arrayIndex)
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
		/// Removes the first occurrence of a specific object from the collection.
		/// </summary>
		/// <param name="item">The object to remove from the collection.</param>
		/// <returns>
		/// true if item was successfully removed from the collection; otherwise, false. 
		/// This method also returns false if item is not found in the original collection.
		/// </returns>
		public bool Remove(Tuple<T1, T2> item)
		{
			// To remove the matching pair of items, the primary collection needs the RemoveAt method,
			// which isn't supported by ICollection<T>.
			IList<T1> primaryList = m_primaryCollection as IList<T1>;
			if(primaryList == null)
				throw new NotSupportedException("The primary collection doesn't support removing the item.");

			try
			{
				IgnoreCollectionChanging = true;

				for(int index=0; index<primaryList.Count; index++)
				{
					bool equals = EqualityComparer<T1>.Default.Equals(primaryList[index], item.Item1);
					if(!equals)
						continue;

					equals = EqualityComparer<T2>.Default.Equals(m_secondaryCollection[index], item.Item2);
					if(!equals)
						continue;

					primaryList.RemoveAt(index);
					m_secondaryCollection.RemoveAt(index);

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
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		/// <returns>An enumerator that can be used to iterate through the collection.</returns>
		IEnumerator<Tuple<T1, T2>> IEnumerable<Tuple<T1, T2>>.GetEnumerator()
		{
			DualEnumerator<T1, T2> enumerator = new DualEnumerator<T1, T2>(m_primaryCollection, m_secondaryCollection);
			return enumerator;
		}
		#endregion

		#region IEnumerable implementation
		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns>
		/// An System.Collections.IEnumerator object that can be used to iterate through the collection.
		/// </returns>
		public IEnumerator GetEnumerator()
		{
			IEnumerator dualEnumerator = new DualEnumerator<T1, T2>(m_primaryCollection, m_secondaryCollection);
			return dualEnumerator;
		}
		#endregion

		#region Properties
		/// <summary>
		/// Delegate used to create an object in the secondary collection,
		/// in response to an object in the primary collection being added.
		/// </summary>
		/// <param name="index">Index being created or modified.</param>
		/// <param name="primaryValue">Object being created or modified in the other collection.</param>
		/// <returns>Object to add to the secondary collection.</returns>
		public delegate T2 CreateDelegate(int index, T1 primaryValue);

		/// <summary>
		/// Delegate used to update an object in the secondary collection, 
		/// in response to an object in the primary collection being modified.
		/// </summary>
		/// <param name="index">Index being modified.</param>
		/// <param name="primaryValue">Value being set on the primary collection.</param>
		/// <param name="currentValue">Current value of the object in the secondary collection.</param>
		/// <returns>Value to set on the object in the secondary collection.</returns>
		public delegate T2 UpdateDelegate(int index, T1 primaryValue, T2 currentValue);

		/// <summary>
		/// Implementation to use to create an object in the secondary collection, when 
		/// the primary collection grows.
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

				// Now that it's possible to create secondary elements, fill out the secondary collection
				// with elements, such that it contains as many elements as the primary collection.
				SynchronizeCollectionSizes();
			}
		}

		/// <summary>
		/// Implementation to use to modify an object in the secondary collection, when 
		/// the primary collection is modified.
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
		/// Indicates if the secondary collection is being modified in response to the 
		/// event handler of the primary collection.
		/// </summary>
		private bool SecondaryCollectionLocked
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates that the primary and secondary collections are both about to change and
		/// to not execute the event handler of the primary collection.
		/// </summary>
		private bool IgnoreCollectionChanging
		{
			get;
			set;
		}
		#endregion

		#region Event handlers
		/// <summary>
		/// Updates the secondary collection in response to the primary collection being updated.
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
						// If it wasn't possible to update the secondary collection to match the primary collection.
						if(CreateSecondary == null)
							return;

						for(int count = 0; count<e.NewItems.Count; count++)
						{
							object matchingObject = CreateSecondary(e.NewStartingIndex+count, (T1) e.NewItems[count]);
							m_secondaryCollection.Insert(e.NewStartingIndex+count, (T2) matchingObject);
						}
					}
					break;

					case NotifyCollectionChangedAction.Move:
					{
						List<T2> removedObjects = new List<T2>(e.OldItems.Count);

						for(int count = 0; count<e.OldItems.Count; count++)
						{
							removedObjects.Add(m_secondaryCollection[e.OldStartingIndex]);
							m_secondaryCollection.RemoveAt(e.OldStartingIndex);
						}

						for(int count = 0; count<e.NewItems.Count; count++)
							m_secondaryCollection.Insert(e.NewStartingIndex+count, removedObjects[count]);
					}
					break;

					case NotifyCollectionChangedAction.Remove:
					{
						for(int count=0; count<e.OldItems.Count; count++)
							m_secondaryCollection.RemoveAt(e.OldStartingIndex+count);
					}
					break;

					case NotifyCollectionChangedAction.Replace:
					{
						// If it wasn't possible to update the secondary collection to match the primary collection.
						if(UpdateSecondary == null)
							return;

						for(int count = 0; count<e.NewItems.Count; count++)
						{
							object matchingObject = UpdateSecondary(e.NewStartingIndex+count, (T1) e.NewItems[count], m_secondaryCollection[count]);
							m_secondaryCollection[e.NewStartingIndex+count] = (T2) matchingObject;
						}
					}
					break;

					case NotifyCollectionChangedAction.Reset:
					{
						m_secondaryCollection.Clear();
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
		/// Updates the primary collection in response to the secondary collection being updated.
		/// </summary>
		private void SecondaryCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			if(IgnoreCollectionChanging)
				return;

			if(SecondaryCollectionLocked)
				return;

			throw new InvalidOperationException("Modifying the SecondaryCollection is not permitted, while it's part of the DualCollection.");
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
		/// <param name="primaryValue">Value being set on the primary collection.</param>
		/// <param name="currentValue">Current value of the object in the secondary collection.</param>
		/// <returns>Value to set on the object in the secondary collection.</returns>
		private T2 UpdateSecondaryDefault(int index, T1 primaryValue, T2 currentValue)
		{
			return default(T2);
		}

		/// <summary>
		/// Fill out the secondary collection with values, until the secondary collection contains as many values
		/// as the primary collection.
		/// 
		/// If CreateSecondary is null, this method does nothing.
		/// </summary>
		private void SynchronizeCollectionSizes()
		{
			if(CreateSecondary == null)
				return;

			if(m_primaryCollection.Count < m_secondaryCollection.Count)
			{
				// Remove extra elements from the secondary collection, until the secondary collection
				// only contains as many elements as the primary collection.
				while(m_secondaryCollection.Count > m_primaryCollection.Count)
					m_secondaryCollection.RemoveAt(m_primaryCollection.Count);
			}

			if(m_primaryCollection.Count > m_secondaryCollection.Count)
			{
				// Add missing elements to the secondary collection, until the secondary collection
				// contains as many elements as the primary collection.
				int index = m_secondaryCollection.Count;
				IEnumerable<T1> primaryEnumerator = m_primaryCollection.Skip(index);
				foreach(T1 primaryValue in primaryEnumerator)
				{
					T2 secondaryValue = CreateSecondary(index, primaryValue);
					m_secondaryCollection.Add(secondaryValue);
				}
			}
		}
		#endregion

		#region Fields
		ICollection<T1>		m_primaryCollection;
		IList<T2>			m_secondaryCollection;
		CreateDelegate		m_createSecondary;
		UpdateDelegate		m_updateSecondary;
		#endregion
	}
}
