using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BindingTestConsoleApplication
{
	public class FirstClass	:INotifyPropertyChanged
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
		/// First property.
		/// </summary>
		public string FirstProperty
		{
			get
			{
				return m_firstProperty;
			}
			set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_firstProperty, value, NotifyPropertyChanged);
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
		/// Backing field for the <see cref="Other"/> property.
		/// </summary>
		private object m_other;

		/// <summary>
		/// Backing field for the <see cref="FirstProperty"/> property.
		/// </summary>
		private string m_firstProperty;
	}
}
