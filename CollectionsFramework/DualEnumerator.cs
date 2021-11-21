using System;
using System.Collections;

namespace Collections
{
	/// <summary>
	/// Enumerates two collections in sync.
	/// </summary>
	public class DualEnumerator :IEnumerator
	{
		public DualEnumerator(IEnumerable primaryCollection, IEnumerable secondaryCollection)
		{
			//m_primaryCollection		= primaryCollection;
			//m_secondaryCollection	= secondaryCollection;

			m_primaryEnumerator		= primaryCollection.GetEnumerator();
			m_secondaryEnumerator	= secondaryCollection.GetEnumerator();
		}

		/// <summary>
		/// Gets the current element in the collection.
		/// </summary>
		public object Current
		{
			get
			{
				Tuple<object, object> result = new Tuple<object, object>(m_primaryEnumerator.Current, m_secondaryEnumerator.Current);
				return result;
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

		//IEnumerable m_primaryCollection,
		//			m_secondaryCollection;

		IEnumerator	m_primaryEnumerator,
					m_secondaryEnumerator;
	}
}
