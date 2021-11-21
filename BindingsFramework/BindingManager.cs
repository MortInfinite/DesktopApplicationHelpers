using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace Bindings
{
	public class BindingManager	:IDisposable
	{
		#region IDisposable Members
		/// <summary>
		/// Dispose of the object and its unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Dispose pattern implementation.
		/// </summary>
		/// <param name="disposing">True if disposing, false if finalizing.</param>
		protected virtual void Dispose(bool disposing)
		{
			if(Disposed)
				return;

			// TODO: Dispose unmanaged code here.

			if(disposing)
			{
				Clear();
			}

			Disposed = true;
		}

		/// <summary>
		/// Indicates if the object has been disposed.
		/// </summary>
		public bool Disposed
		{
			get;
			protected set;
		}
		#endregion

		/// <summary>
		/// Create a one way binding between the property on the source object, to the property on the destination object.
		/// </summary>
		/// <param name="sourceObject">Object to retrieve the source property from.</param>
		/// <param name="sourceProperty">Name of the source property to retrieve.</param>
		/// <param name="destinationObject">Object to set the destination property on.</param>
		/// <param name="destinationProperty">Name of the destination property to set.</param>
		public Binding AddOneWayBinding(INotifyPropertyChanged sourceObject, string sourceProperty, object destinationObject, string destinationProperty)
		{
			if(Disposed)
				throw new ObjectDisposedException(nameof(BindingManager));

			Binding binding = new Binding(sourceObject, sourceProperty, destinationObject, destinationProperty, BindingModes.OneWay);

			Bindings.Add(binding);

			return binding;
		}

		/// <summary>
		/// Create a two way binding between the property on the source object, to the property on the destination object.
		/// </summary>
		/// <param name="sourceObject">Object to retrieve the source property from.</param>
		/// <param name="sourceProperty">Name of the source property to retrieve.</param>
		/// <param name="destinationObject">Object to set the destination property on.</param>
		/// <param name="destinationProperty">Name of the destination property to set.</param>
		public Binding AddTwoWayBinding(INotifyPropertyChanged sourceObject, string sourceProperty, object destinationObject, string destinationProperty)
		{
			if(Disposed)
				throw new ObjectDisposedException(nameof(BindingManager));

			Binding binding = new Binding(sourceObject, sourceProperty, destinationObject, destinationProperty, BindingModes.TwoWay);

			Bindings.Add(binding);

			return binding;
		}

		public void Clear()
		{
			foreach(var binding in Bindings)
				binding.Dispose();

			Bindings.Clear();
		}

		protected virtual HashSet<Binding> Bindings
		{
			get;
		} = new HashSet<Binding>();
	}
}
