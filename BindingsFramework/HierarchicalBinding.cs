using System;
using System.ComponentModel;
using System.Reflection;
using System.Linq;
using System.Diagnostics;

namespace Bindings
{
	/// <summary>
	/// Creates a binding between the property on the source object and a property on the destination object.
	/// 
	/// For one way or two bindings, the destination property will be updated, when the source property changes.
	/// 
	/// For two way bindings, the source property also be updated, when the destination property changes.
	/// </summary>
	public class HierarchicalBinding	:IDisposable
	{
		/// <summary>
		/// Create a binding between the property on the source object and a property on the destination object.
		/// </summary>
		/// <param name="sourceObject">Object to retrieve the source property from.</param>
		/// <param name="sourcePath">Name of the source property to retrieve.</param>
		/// <param name="destinationObject">Object to set the destination property on.</param>
		/// <param name="destinationPath">Name of the destination property to set.</param>
		/// <param name="bindingMode">Indicates whether to copy properties to the source object, destination object or both.</param>
		public HierarchicalBinding(object sourceObject, string sourcePath, object destinationObject, string destinationPath, BindingModes bindingMode, object fallbackValue=null)
		{
			if(sourceObject == null)
				throw new ArgumentNullException(nameof(sourceObject));
			if(destinationObject == null)
				throw new ArgumentNullException(nameof(destinationObject));
			if(string.IsNullOrEmpty(sourcePath))
				throw new ArgumentNullException(nameof(sourcePath));
			if(string.IsNullOrEmpty(destinationPath))
				throw new ArgumentNullException(nameof(destinationPath));

			string[] sourcePathParts = sourcePath.Split('.');
			foreach(string sourcePathPart in sourcePathParts)
				if(string.IsNullOrEmpty(sourcePathPart))
					throw new ArgumentException($"The path \"{sourcePath}\" is not valid.", nameof(sourcePath));

			string[] destinationPathParts = destinationPath.Split('.');
			foreach(string destinationPathPart in destinationPathParts)
				if(string.IsNullOrEmpty(destinationPathPart))
					throw new ArgumentException($"The path \"{destinationPath}\" is not valid.", nameof(destinationPath));

			SourceObject			= sourceObject;
			SourcePath				= sourcePath;
			DestinationObject		= destinationObject;
			DestinationPath			= destinationPath;
			BindingMode				= bindingMode;
			FallbackValue			= fallbackValue;

			SourceBindingParts		= CreateBindingParts(sourcePathParts);
			DestinationBindingParts = CreateBindingParts(destinationPathParts);

			// Update the chain of source binding parts and remember the resolved property value of the last element in the chain.
			object sourceValue = UpdateBindingPartSourceObject(SourceBindingParts[0], sourceObject);
			if(sourceValue == UnresolvedValue)
				sourceValue = FallbackValue;

			UpdateBindingPartSourceObject(DestinationBindingParts[0], destinationObject);

			// Copy the value from the source property to the destination property.
			UpdatePropertyValue(DestinationBindingParts[DestinationBindingParts.Length-1], sourceValue);
		}

		#region Types
		[DebuggerDisplay("{SourceObject?.GetType().Name,nq}.{PropertyName,nq} = {ResolvedValue}")]
		protected class BindingPart
		{
			#region Properties
			public string PropertyName
			{
				get;
				set;
			}

			public object SourceObject
			{
				get;
				set;
			}

			public PropertyInfo PropertyInfo
			{
				get;
				set;
			}

			public DependencyPropertyDescriptor DependencyPropertyDescriptor
			{
				get; 
				set;
			}

			public EventHandler DependencyPropertyEventHandler
			{
				get; 
				set;
			}

			public bool CanGet
			{
				get;
				set;
			}

			public bool CanSet
			{
				get;
				set;
			}

			public BindingPart NextBindingPart
			{
				get;
				set;
			}

			public object ResolvedValue
			{
				get;
				set;
			}
			#endregion
		}
		#endregion

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
			lock(LockObject)
			{
				if(Disposed)
					return;

				if(disposing)
				{
					foreach(BindingPart bindingPart in this.SourceBindingParts)
						if(bindingPart.SourceObject is INotifyPropertyChanged previousSourceObject)
							previousSourceObject.PropertyChanged -= BindingPart_PropertyChanged;

					foreach(BindingPart bindingPart in this.DestinationBindingParts)
						if(bindingPart.SourceObject is INotifyPropertyChanged previousSourceObject)
							previousSourceObject.PropertyChanged -= BindingPart_PropertyChanged;

					SourceBindingParts		= null;
					DestinationBindingParts	= null;
				}

				Disposed = true;
			}
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

		#region Properties
		/// <summary>
		/// Object to retrieve the source property from.
		/// </summary>
		public object SourceObject
		{
			get;
			protected set;
		}

		/// <summary>
		/// Name of the source property to retrieve.
		/// </summary>
		public string SourcePath
		{
			get;
			protected set;
		}

		/// <summary>
		/// Object to set the destination property on.
		/// </summary>
		public object DestinationObject
		{
			get;
			protected set;
		}

		/// <summary>
		/// Name of the destination property to set.
		/// </summary>
		public string DestinationPath
		{
			get;
			protected set;
		}

		/// <summary>
		/// Indicates whether to copy properties to the source object, destination object or both.
		/// </summary>
		public BindingModes BindingMode
		{
			get;
			protected set;
		}

		/// <summary>
		/// Object used to lock methods for use with a single thread at the time.
		/// </summary>
		protected object LockObject
		{
			get;
		} = new object();

		/// <summary>
		/// Value to set when binding fails.
		/// </summary>
		public object FallbackValue
		{
			get;
			protected set;
		}

		/// <summary>
		/// Parts that the source binding is split into. 
		/// 
		/// The binding parts are created by splitting the <see cref="SourcePath"/> into individual properties, separated by a ".".
		/// </summary>
		protected BindingPart[] SourceBindingParts
		{
			get;
			set;
		}

		/// <summary>
		/// Parts that the source binding is split into. 
		/// 
		/// The binding parts are created by splitting the <see cref="DestinationPath"/> into individual properties, separated by a ".".
		/// </summary>
		protected BindingPart[] DestinationBindingParts
		{
			get;
			set;
		}
		#endregion

		#region Event handlers
		/// <summary>
		/// Copy the source property value to the destination property, when the source property changes.
		/// </summary>
		/// <param name="sender">Source object on which the property changed.</param>
		/// <param name="e">Name of the property that changed.</param>
		protected virtual void BindingPart_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			lock(LockObject)
			{
				if(Disposed)
					return;

				// When a binding part match is found, that binding and all bindings following it, 
				// must be updated, to match the new objects they refer to.
				for(int count=0; count<SourceBindingParts.Length; count++)
				{
					BindingPart bindingPart = SourceBindingParts[count];

					// If the property changed event came from the source object of the current binding part.
					if(bindingPart.SourceObject == sender && e.PropertyName == bindingPart.PropertyName)
					{
						// Update the binding part and any binding parts following it.
						object resolvedValue = UpdateBindingPart(bindingPart);
						if(resolvedValue == ResolvedObjectUnchanged)
							return;

						if(resolvedValue == UnresolvedValue)
							resolvedValue = FallbackValue;

						// Copy the value from the source property to the destination property.
						UpdatePropertyValue(DestinationBindingParts[DestinationBindingParts.Length-1], resolvedValue);

						return;
					}
				}

				// When a binding part match is found, that binding and all bindings following it, 
				// must be updated, to match the new objects they refer to.
				for(int count=0; count<DestinationBindingParts.Length; count++)
				{
					BindingPart bindingPart = DestinationBindingParts[count];

					// If the property changed event came from the source object of the current binding part.
					if(bindingPart.SourceObject == sender && e.PropertyName == bindingPart.PropertyName)
					{
						// Update the binding part and any binding parts following it.
						// Retrieve the current value of the last binding part.
						object resolvedValue = UpdateBindingPart(bindingPart);
						if(resolvedValue == ResolvedObjectUnchanged || resolvedValue == UnresolvedValue)
							return;
					
						// Find the last source binding part.
						BindingPart finalSourceBindingPart		= SourceBindingParts[SourceBindingParts.Length-1];

						// If using a two way binding, copy the value from the destination property to the source property.
						if(BindingMode == BindingModes.TwoWay)
							UpdatePropertyValue(finalSourceBindingPart, resolvedValue);
						else
						{
							BindingPart finalDestinationBindingPart = DestinationBindingParts[DestinationBindingParts.Length-1];

							// Retrieve the resolved value from the source binding.
							resolvedValue = finalSourceBindingPart?.ResolvedValue;
							if(resolvedValue == UnresolvedValue)
								resolvedValue = FallbackValue;

							UpdatePropertyValue(finalDestinationBindingPart, resolvedValue);
						}

						return;
					}
				}
			}
		}

		#endregion

		#region Methods
		/// <summary>
		/// Create an array of binding parts and assign the property name of each created binding part.
		/// 
		/// The first created binding part will have its <see cref="BindingPart.SourceObject"/> property set 
		/// to the specified <paramref name="sourceObject"/>.
		/// </summary>
		/// <param name="sourceObject">Source object to set on the first created binding part.</param>
		/// <param name="pathParts">Property names to assign to the created binding parts.</param>
		/// <returns>Created array of binding parts.</returns>
		protected virtual BindingPart[] CreateBindingParts(string[] pathParts)
		{
			BindingPart[] bindingParts = new BindingPart[pathParts.Length];

			for(int count=0; count<pathParts.Length; count++)
			{
				bindingParts[count]					= new BindingPart();
				bindingParts[count].SourceObject	= Uninitialized;
				bindingParts[count].PropertyName	= pathParts[count];

				if(count > 0)
					bindingParts[count-1].NextBindingPart = bindingParts[count];
			}

			return bindingParts;
		}

		/// <summary>
		/// Retrieves the value of the specified binding part.
		/// 
		/// If another binding part depends on the value of the specified binding part, the other binding part's
		/// source object is updated, resulting in a cascading update of all following binding parts.
		/// 
		/// When the chain of binding parts have been updated, the value of the final binding part is returned.
		/// </summary>
		/// <param name="bindingPart">Binding part who's property has changed.</param>
		/// <returns>Resolved value of the final binding part, in the chain of depending binding parts.</returns>
		protected object UpdateBindingPart(BindingPart bindingPart)
		{
			// Retrieve the resolved value of the binding part.
			if(bindingPart.SourceObject != null && bindingPart.CanGet)
				bindingPart.ResolvedValue = bindingPart.PropertyInfo.GetValue(bindingPart.SourceObject);
			else
				bindingPart.ResolvedValue = UnresolvedValue;

			// If there is a binding part after this one, update the next binding part with a new
			// source object and resolve the binding part to be able to retrieve it's property value.
			if(bindingPart.NextBindingPart != null)
				return UpdateBindingPartSourceObject(bindingPart.NextBindingPart, bindingPart.ResolvedValue);

			return bindingPart.ResolvedValue;
		}

		/// <summary>
		/// Update the SourceObject, of the specified binding part, and update any properties and event subscriptions
		/// depending on the source object.
		/// </summary>
		protected virtual object UpdateBindingPartSourceObject(BindingPart bindingPart, object sourceObject)
		{
			if(Disposed)
				throw new ObjectDisposedException(nameof(HierarchicalBinding));

			// If the source object hasn't changed, neither have any of the next binding parts.
			if(sourceObject == bindingPart.SourceObject)
				return ResolvedObjectUnchanged;

			// Unsubscribe from property changed events from the current binding part values.
			UnsubscribeBindingPart(bindingPart);

			// Subscribe to property changed events from the current binding part values.
			SubscribeBindingPart(bindingPart, sourceObject);

			// If there is a binding part after this one, update the next binding part with a new source object.
			if(bindingPart.NextBindingPart != null)
				return UpdateBindingPartSourceObject(bindingPart.NextBindingPart, bindingPart.ResolvedValue);

			return bindingPart.ResolvedValue;
		}

		/// <summary>
		/// Unsubsribe from property changed events for the specified binding part.
		/// </summary>
		/// <param name="bindingPart">BindingPart to unsubscribe from.</param>
		protected virtual void UnsubscribeBindingPart(BindingPart bindingPart)
		{
			// If the source object has been updated, unsubscribe from property change notifications 
			// from the previous source object.
			if(bindingPart.SourceObject is INotifyPropertyChanged previousSourceObject)
				previousSourceObject.PropertyChanged -= BindingPart_PropertyChanged;
			else if(bindingPart.DependencyPropertyDescriptor != null && bindingPart.DependencyPropertyEventHandler != null)
			{
				bindingPart.DependencyPropertyDescriptor.RemoveValueChanged(bindingPart.SourceObject, bindingPart.DependencyPropertyEventHandler);
				
				bindingPart.DependencyPropertyDescriptor	= null;
				bindingPart.DependencyPropertyEventHandler	= null;
			}
		}

		/// <summary>
		/// Subsribe to property changed events from the specified binding part.
		/// </summary>
		/// <param name="bindingPart">BindingPart to subscribe to.</param>
		protected virtual void SubscribeBindingPart(BindingPart bindingPart, object sourceObject)
		{
			bindingPart.SourceObject	= sourceObject;
			bindingPart.PropertyInfo	= null;
			bindingPart.CanGet			= false;
			bindingPart.CanSet			= false;
			bindingPart.ResolvedValue	= UnresolvedValue;

			if(sourceObject != null && sourceObject != UnresolvedValue)
			{
				Type			sourceObjectType	= sourceObject.GetType();
				PropertyInfo	propertyInfo		= sourceObjectType.GetProperty(bindingPart.PropertyName);

				// If the property exists.
				if(propertyInfo != null)
				{
					MethodInfo	getAccessor	= propertyInfo.GetGetMethod();
					MethodInfo	setAccessor	= propertyInfo.GetSetMethod();

					//bindingPart.SourceObject	= sourceObject;
					bindingPart.PropertyInfo	= propertyInfo;

					// Check if the source property can be read.
					if(getAccessor != null && getAccessor.IsPublic)
						bindingPart.CanGet		= true;
					else
						bindingPart.CanGet		= false;

					// Check if the source property can be set.
					if(setAccessor != null && setAccessor.IsPublic)
						bindingPart.CanSet		= true;
					else
						bindingPart.CanSet		= false;

					// Check that we can subscribe to change events from the source object.
					// Unsubscribe from the source object's property changed notification.
					if(bindingPart.SourceObject is INotifyPropertyChanged newSourceObject)
						newSourceObject.PropertyChanged += BindingPart_PropertyChanged;
					else
					{
						// Determine if the property exists as a dependency property.
						PropertyDescriptorCollection propertyDescriptors = TypeDescriptor.GetProperties(bindingPart.SourceObject, new Attribute[] { new PropertyFilterAttribute(PropertyFilterOptions.All)});
						foreach(PropertyDescriptor propertyDescriptor in propertyDescriptors)
						{
							// Skip properties that don't match the property name.
							if(propertyDescriptor.Name != bindingPart.PropertyName)
								continue;
					
							// Find the property descriptor of the dependency property.
							// If the property descriptor isn't found, the property isn't a dependency property.
							DependencyPropertyDescriptor dependencyPropertyDescriptor	= DependencyPropertyDescriptor.FromProperty(propertyDescriptor);
							if(dependencyPropertyDescriptor != null)
							{
								// Remember the dependency property descriptor and event handler, so we can unsubscribe from them again.
								bindingPart.DependencyPropertyDescriptor	= dependencyPropertyDescriptor;
								bindingPart.DependencyPropertyEventHandler	= (sender, unused) => {BindingPart_PropertyChanged(sender, new PropertyChangedEventArgs(bindingPart.PropertyName));};

								// Listen to property changed events from the dependency property.
								dependencyPropertyDescriptor.AddValueChanged(bindingPart.SourceObject, bindingPart.DependencyPropertyEventHandler);
							}

							// Don't look for more properties, now that we found the one matching the property name.
							break;
						}
					}
				}
			}

			// Retrieve the resolved value of the binding part.
			if(bindingPart.SourceObject != null && bindingPart.CanGet)
				bindingPart.ResolvedValue = bindingPart.PropertyInfo.GetValue(bindingPart.SourceObject);
			else
				bindingPart.ResolvedValue = UnresolvedValue;
		}

		/// <summary>
		/// Update the destination property with the specified value.
		/// </summary>
		protected virtual void UpdatePropertyValue(BindingPart bindingPart, object value)
		{
			if(Disposed)
				throw new ObjectDisposedException(nameof(HierarchicalBinding));

			try
			{
				// If we can't set values on the binding part.
				if(!bindingPart.CanSet)
					return;

				if(bindingPart.CanGet)
				{
					// If the destination value is the same as the source value, don't update the property.
					object currentValue = bindingPart.PropertyInfo.GetValue(bindingPart.SourceObject);
					if(object.Equals(value, currentValue))
						return;
				}

				// Update the binding part value.
				bindingPart.PropertyInfo.SetValue(bindingPart.SourceObject, value);
			}
			catch
			{
			}
		}
		#endregion

		#region Fields
		/// <summary>
		/// Value indicates that the binding value hierarchy is unchanged.
		/// </summary>
		private static readonly object ResolvedObjectUnchanged	= new object();

		/// <summary>
		/// Value indicates that the binding could not be resolved.
		/// 
		/// This can happen if the binding element's SourceObject is null or if the binding property's
		/// Get accessor isn't available.
		/// </summary>
		private static readonly object UnresolvedValue			= new object();

		/// <summary>
		/// Value indicates that the source object hasn't been initialized yet.
		/// </summary>
		/// <remarks>
		/// Setting the BindingPart.SourceObject to Uninitialized, when it's first created, ensures
		/// that the BindingPart gets updated, the first time <see cref="UpdateBindingPartSourceObject"/> is called.
		/// </remarks>
		private static readonly object Uninitialized			= new object();
		#endregion
	}
}
