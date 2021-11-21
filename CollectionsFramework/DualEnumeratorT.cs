using System;
using System.Collections;
using System.Collections.Generic;

namespace Collections
{
	/// <summary>
	/// Enumerates two collections in sync.
	/// </summary>
	public class DualEnumerator<T1, T2> :IEnumerator<Tuple<T1, T2>>
	{
		public DualEnumerator(IEnumerable<T1> primaryCollection, IEnumerable<T2> secondaryCollection)
		{
			m_primaryEnumerator		= primaryCollection.GetEnumerator();
			m_secondaryEnumerator	= secondaryCollection.GetEnumerator();
		}

		#region IEnumerator<T>
		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get
			{
				Tuple<T1, T2> result = new Tuple<T1, T2>(m_primaryEnumerator.Current, m_secondaryEnumerator.Current);
				return result;
			}
		}

		/// <summary>
		/// Gets the element in the collection at the current position of the enumerator.
		/// </summary>
		Tuple<T1, T2> IEnumerator<Tuple<T1, T2>>.Current
		{
			get
			{
				return (Tuple<T1, T2>) Current;
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
			bool result = m_primaryEnumerator.MoveNext();
			m_secondaryEnumerator.MoveNext();

			return result;
		}

		/// <summary>
		/// Sets the enumerator to its initial position, which is before the first element in the collection.
		/// </summary>
		/// <exception cref="InvalidOperationException">The collection was modified after the enumerator was created.</exception>
		public void Reset()
		{
			m_primaryEnumerator.Reset();
			m_secondaryEnumerator.Reset();
		}

		/// <summary>
		/// Disposes the enumerator.
		/// </summary>
		public void Dispose()
		{
			m_primaryEnumerator.Dispose();
			m_secondaryEnumerator.Dispose();
		}
		#endregion

		#region Private fields
		IEnumerator<T1>	m_primaryEnumerator;
		IEnumerator<T2>	m_secondaryEnumerator;
		#endregion
	}
}
