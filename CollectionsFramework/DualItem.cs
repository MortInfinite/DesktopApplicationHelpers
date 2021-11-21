using System;
using System.Collections.Generic;

namespace Collections
{
	public class DualItem<T1, T2>
	{
		public DualItem(IList<T1> primaryCollection, IList<T2> secondaryCollection, int index)
		{
			if(primaryCollection == null)
				throw new ArgumentNullException(nameof(primaryCollection));
			if(secondaryCollection == null)
				throw new ArgumentNullException(nameof(secondaryCollection));

			m_primaryList	= primaryCollection;
			m_secondaryList	= secondaryCollection;
			m_index			= index;
		}

		public T1 Primary
		{
			get
			{
				return m_primaryList[m_index];
			}
			set
			{
				m_primaryList[m_index] = value;
			}
		}

		public T2 Secondary
		{
			get
			{
				return m_secondaryList[m_index];
			}
			set
			{
				m_secondaryList[m_index] = value;
			}
		}

		IList<T1>	m_primaryList;
		IList<T2>	m_secondaryList;
		int			m_index;
	}
}
