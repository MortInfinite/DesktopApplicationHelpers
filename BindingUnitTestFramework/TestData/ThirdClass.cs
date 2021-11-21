using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TestData
{
	public class ThirdClass	:INotifyPropertyChanged
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
		/// Third property.
		/// </summary>
		public string ThirdProperty
		{
			get
			{
				return m_thirdProperty;
			}
			set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_thirdProperty, value, NotifyPropertyChanged);
			}
		}

		/// <summary>
		/// Reference to another object.
		/// </summary>
		public FirstClass First
		{
			get
			{
				return m_first;
			}
			set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_first, value, NotifyPropertyChanged);
			}
		}

		/// <summary>
		/// Backing field for the <see cref="First"/> property.
		/// </summary>
		private FirstClass m_first;

		/// <summary>
		/// Backing field for the <see cref="ThirdProperty"/> property.
		/// </summary>
		private string m_thirdProperty;
	}
}
