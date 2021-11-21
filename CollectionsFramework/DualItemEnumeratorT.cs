using System;
using System.Collections;
using System.Collections.Generic;

namespace Collections
{
	/// <summary>
	/// Enumerates two collections in sync.
	/// </summary>
	public class DualItemEnumerator<T1, T2> :IEnumerator<DualItem<T1, T2>>
	{
		public DualItemEnumerator(IList<T1> primaryCollection, IList<T2> secondaryCollection)
		{
			m_primaryList			= primaryCollection;
			m_secondaryList			= secondaryCollection;
			//m_primaryEnumerator		= primaryCollection.GetEnumerator();
			//m_secondaryEnumerator	= secondaryCollection.GetEnumerator();
		}

		#region IEnumerator<T>
		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get
			{
				DualItem<T1, T2> result = new DualItem<T1, T2>(m_primaryList, m_secondaryList, m_index);
				return result;
			}
		}

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		DualItem<T1, T2> IEnumerator<DualItem<T1, T2>>.Current
		{
			get
			{
				return (DualItem<T1, T2>) Current;
			}
		}

		/// <summary>
		/// Advances the enumerator to the next element of the collection.
		/// </summary>
		/// <returns>
		/// true if the enumerator was successfully advanced to the next element; 
		/// false if the enumerator has passed the end of the collection.
		/// </returns>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public bool MoveNext()
		{
			//bool result = m_primaryEnumerator.MoveNext();
			//if(!result)
			//	return false;

			//result = m_secondaryEnumerator.MoveNext();
			//if(!result)
			//	return false;

			m_index++;

			return true;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public void Reset()
		{
			m_index = 0;
			//m_primaryEnumerator.Reset();
			//m_secondaryEnumerator.Reset();
		}

		/// <summary>
		/// Disposes the enumerator.
		/// </summary>
		public void Dispose()
		{
			//m_primaryEnumerator.Dispose();
			//m_secondaryEnumerator.Dispose();
		}
		#endregion

		#region Private fields
		IList<T1>		m_primaryList;
		IList<T2>		m_secondaryList;
		//IEnumerator<T1>	m_primaryEnumerator;
		//IEnumerator<T2>	m_secondaryEnumerator;
		int				m_index					= 0;
		#endregion
	}
}
