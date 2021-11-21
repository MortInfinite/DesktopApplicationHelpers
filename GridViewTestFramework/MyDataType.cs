using System.ComponentModel;

namespace GridViewTest
{
	public class MyDataType	:INotifyPropertyChanged
	{
		public MyDataType(string first, string second, string third)
		{
			First	= first;
			Second	= second;
			Third	= third;
		}

		#region INotifyPropertyChanged implementation
		public event PropertyChangedEventHandler PropertyChanged;
		#endregion

		/// <summary>
		/// Description for this property.
		/// </summary>
		public string First
		{
			get
			{
				return m_first;
			}
			set
			{
				// Don't set the property to its current value.
				if(value == m_first)
					return;

				m_first = value;

				// Notify subscribers that the property changed.
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(First)));
			}
		}

		public string Second
		{
			get
			{
				return m_second;
			}
			set
			{
				// Don't set the property to its current value.
				if(value == m_second)
					return;

				m_second = value;

				// Notify subscribers that the property changed.
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Second)));
			}
		}

		public string Third
		{
			get
			{
				return m_third;
			}
			set
			{
				// Don't set the property to its current value.
				if(value == m_third)
					return;

				m_third = value;

				// Notify subscribers that the property changed.
				PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Third)));
			}
		}

		/// <summary>
		/// Backing field for the First property.
		/// </summary>
		private string m_first;

		/// <summary>
		/// Backing field for the Second property.
		/// </summary>
		private string m_second;

		/// <summary>
		/// Backing field for the Third property.
		/// </summary>
		private string m_third;
	}
}
