using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collections
{
	/// <summary>
	/// Delegate used to indicate that a property value has changed, in an element contained in a collection.
	/// </summary>
	/// <param name="sender">Collection containing the element.</param>
	/// <param name="element">Element on which the property has changed.</param>
	/// <param name="propertyName">Name of the property that changed.</param>
	public delegate void CollectionPropertyChangedDelegate(object sender, object element, string propertyName);

	/// <summary>
	/// Notifies clients that a property value has changed on an element in contained in a collection.
	/// </summary>
	interface INotifyCollectionPropertyChanged
	{
		/// <summary>
		/// Occurs when a property value changes, in an element contained in a collection.
		/// </summary>
		event CollectionPropertyChangedDelegate CollectionPropertyChanged;
	}
}
