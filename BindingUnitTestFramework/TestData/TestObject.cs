using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestData
{
	/// <summary>
	/// Test object that has a value and a reference to another object.
	/// </summary>
	public class TestObject	:INotifyPropertyChanged
	{
		#region INotifyPropertyChanged
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Notifies subscribers that the property changed.
        /// </summary>
        /// <param name="propertyName">Name of the property that changed.</param>
        protected virtual void NotifyPropertyChanged([CallerMemberName] string propertyName="")
        {
            if(string.IsNullOrEmpty(propertyName))
                throw new ArgumentException($"The {nameof(propertyName)} argument wasn't specified.", nameof(propertyName));

            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Notifies subscribers that the property changed.
        /// </summary>
		/// <param name="sender">Object raising the event.</param>
        /// <param name="e">Name of the property that changed.</param>
		protected virtual void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
            if(e == null)
                throw new ArgumentException($"The {nameof(e)} argument wasn't specified.", nameof(e));

            PropertyChanged?.Invoke(sender, e);
		}
		#endregion

		/// <summary>
		/// String value.
		/// </summary>
		public string Value
		{
			get
			{
				return m_value;
			}
			set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_value, value, NotifyPropertyChanged);
			}
		}

		/// <summary>
		/// Reference to another object.
		/// </summary>
		public object Other
		{
			get
			{
				return m_other;
			}
			set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_other, value, NotifyPropertyChanged);
			}
		}

		/// <summary>
		/// Backing field for the <see cref="Second"/> property.
		/// </summary>
		private object m_other;

		/// <summary>
		/// Backing field for the <see cref="Value"/> property.
		/// </summary>
		private string m_value;
	}
}
