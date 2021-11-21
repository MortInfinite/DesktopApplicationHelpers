using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace GridViewTest
{
	public class MvvmViewModel	:INotifyPropertyChanged
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

		#region Properties
		/// <summary>
		/// First value
		/// </summary>
		public string First
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
		/// Second value
		/// </summary>
		public string Second
		{
			get
			{
				return m_second;
			}
			set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_second, value, NotifyPropertyChanged);
			}
		}
		#endregion

		#region Fields
		/// <summary>
		/// Backing field for the <see cref="First"/> property.
		/// </summary>
		private string m_first = "First value";

		/// <summary>
		/// Backing field for the <see cref="Second"/> property.
		/// </summary>
		private string m_second = "Second value";
		#endregion
	}
}
