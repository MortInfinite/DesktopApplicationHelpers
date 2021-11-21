using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace System.ComponentModel
{
	/// <summary>
	/// Provides extension methods for the INotifyPropertyChanged interface.
	/// </summary>
	public static class NotifyPropertyChangedExtentions
	{
		/// <summary>
		/// Updates the specified field and sends a property changed notification.
		/// </summary>
		/// <typeparam name="T">Type of property to set.</typeparam>
		/// <param name="sender">Object sending the property changed notification.</param>
		/// <param name="field">Field containing the value to set.</param>
		/// <param name="value">Value to set on the field.</param>
		/// <param name="propertyChangedImplementation">Delegate used to raise the property changed notification.</param>
		/// <param name="propertyName">Name of the property that changed.</param>
		/// <returns>Returns true if the property value has changed.</returns>
		public static bool SetProperty<T>(this INotifyPropertyChanged sender, ref T field, T value, PropertyChangedEventHandler propertyChangedImplementation, [CallerMemberName] string propertyName = null)
		{
			// If being called from a non-property.
			if(propertyName == null)
				return false;

			// Don't set to the same value as the current value.
			bool equals = EqualityComparer<T>.Default.Equals(value, field);
            if (equals)
                return false;

            // Update the backing field.
            field = value;

			// Notify subscribers of the property change.
			propertyChangedImplementation?.Invoke(sender, new PropertyChangedEventArgs(propertyName));

			return true;
		}
	}
}
