using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collections
{
	/// <summary>
	/// Synchronizes a secondary list of items with a primary collection of items.
	/// 
	/// When items are added to the primary list, this class calls a delegate to create a matching
	/// item in the secondary list, keeping the lists of the same size.
	/// 
	/// When items are replaced in the primary list, this class calls a delegate to update the matching
	/// value in the secondary list, keeping the lists in sync.
	/// 
	/// The class does not listen to INotifyPropertyChanged events on items in the primary list, so items 
	/// in the primary list can still be modified, without triggering a change in the secondary list.
	/// </summary>
	public class DualCollection :ICollection
	{
		public DualCollection(ICollection primaryCollection, IList secondaryCollection, CreateDelegate createSecondary=null, UpdateDelegate updateSecondary=null)
		{
			if(primaryCollection == null)
				throw new ArgumentNullException(nameof(primaryCollection));
			if(secondaryCollection == null)
				throw new ArgumentNullException(nameof(secondaryCollection));
			if(primaryCollection.Count != secondaryCollection.Count)
				throw new ArgumentException("The primaryCollection and secondaryCollection do not contain the same number of elements.");

			m_primaryCollection		= primaryCollection;
			m_secondaryCollection	= secondaryCollection;
			CreateSecondary			= createSecondary;
			UpdateSecondary			= updateSecondary;

			INotifyCollectionChanged	primaryCollectionChanged = primaryCollection as INotifyCollectionChanged;
			if(primaryCollectionChanged != null)
				primaryCollectionChanged.CollectionChanged  += PrimaryCollectionChanged_CollectionChanged;

			INotifyCollectionChanged	secondaryCollectionChanged = secondaryCollection as INotifyCollectionChanged;
			if(secondaryCollectionChanged != null)
				secondaryCollectionChanged.CollectionChanged  += SecondaryCollectionChanged_CollectionChanged;
		}

		#region ICollection implementation
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
		/// Gets an object that can be used to synchronize access to the System.Collections.ICollection.
		/// </summary>
		public object SyncRoot
		{
			get
			{
				return m_primaryCollection.SyncRoot;
			}
		}

		/// <summary>
		/// Gets a value indicating whether access to the System.Collections.ICollection
		/// is synchronized (thread safe).
		/// </summary>
		public bool IsSynchronized
		{
			get
			{
				return m_primaryCollection.IsSynchronized;
			}
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
		public void CopyTo(Array array, int index)
		{
			int count = 0;

			foreach(object currentItem in this)
				array.SetValue(currentItem, index+count++);
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
			DualEnumerator dualEnumerator = new DualEnumerator(m_primaryCollection, m_secondaryCollection);
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
		public delegate object CreateDelegate(int index, object primaryValue);

		/// <summary>
		/// Delegate used to update an object in the secondary collection, 
		/// in response to an object in the primary collection being modified.
		/// </summary>
		/// <param name="index">Index being modified.</param>
		/// <param name="primaryValue">Value being set on the primary collection.</param>
		/// <param name="currentValue">Current value of the object in the secondary collection.</param>
		/// <returns>Value to set on the object in the secondary collection.</returns>
		public delegate object UpdateDelegate(int index, object primaryValue, object currentValue);

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
		#endregion

		#region Event handlers
		/// <summary>
		/// Updates the secondary collection in response to the primary collection being updated.
		/// </summary>
		private void PrimaryCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
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
							object matchingObject = CreateSecondary(e.NewStartingIndex+count, e.NewItems[count]);
							m_secondaryCollection.Insert(e.NewStartingIndex+count, matchingObject);
						}
					}
					break;

					case NotifyCollectionChangedAction.Move:
					{
						ArrayList removedObjects = new ArrayList(e.OldItems.Count);

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
							object matchingObject = UpdateSecondary(e.NewStartingIndex+count, e.NewItems[count], m_secondaryCollection[count]);
							m_secondaryCollection[e.NewStartingIndex+count] = matchingObject;
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
		private object CreateSecondaryDefault(int index, object primaryValue)
		{
			return null;
		}

		/// <summary>
		/// Creates a null object reference.
		/// </summary>
		/// <param name="index">Index being modified.</param>
		/// <param name="primaryValue">Value being set on the primary collection.</param>
		/// <param name="currentValue">Current value of the object in the secondary collection.</param>
		/// <returns>Value to set on the object in the secondary collection.</returns>
		private object UpdateSecondaryDefault(int index, object primaryValue, object currentValue)
		{
			return null;
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
				int index = -1;

				// Add missing elements to the secondary collection, until the secondary collection
				// contains as many elements as the primary collection.
				foreach(object primaryValue in m_primaryCollection)
				{
					// Skip over values that already exist in the secondary collection.
					if(++index < m_secondaryCollection.Count)
						continue;

					object secondaryValue = CreateSecondary(index, primaryValue);
					m_secondaryCollection.Add(secondaryValue);
				}
			}
		}
		#endregion

		#region Fields
		ICollection		m_primaryCollection;
		IList			m_secondaryCollection;
		CreateDelegate	m_createSecondary;
		UpdateDelegate	m_updateSecondary;
		#endregion
	}
}
