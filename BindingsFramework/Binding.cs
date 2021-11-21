using System;
using System.ComponentModel;
using System.Reflection;

namespace Bindings
{
	/// <summary>
	/// Creates a binding between the property on the source object and a property on the destination object.
	/// 
	/// For one way or two bindings, the destination property will be updated, when the source property changes.
	/// 
	/// For two way bindings, the source property also be updated, when the destination property changes.
	/// </summary>
	public class Binding	:IDisposable
	{
		/// <summary>
		/// Create a binding between the property on the source object and a property on the destination object.
		/// </summary>
		/// <param name="sourceObject">Object to retrieve the source property from.</param>
		/// <param name="sourcePropertyName">Name of the source property to retrieve.</param>
		/// <param name="destinationObject">Object to set the destination property on.</param>
		/// <param name="destinationPropertyName">Name of the destination property to set.</param>
		/// <param name="bindingMode">Indicates whether to copy properties to the source object, destination object or both.</param>
		public Binding(object sourceObject, string sourcePropertyName, object destinationObject, string destinationPropertyName, BindingModes bindingMode)
		{
			SourceObject			= sourceObject;
			SourcePropertyName		= sourcePropertyName;
			DestinationObject		= destinationObject;
			DestinationPropertyName	= destinationPropertyName;
			BindingMode				= bindingMode;

			// Set up the binding.
			Initialize();

			// Copy the value from the source property to the destination property.
			UpdateDestinationProperty();
		}

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
					INotifyPropertyChanged	notifySourcePropertyChanged			= SourceObject as INotifyPropertyChanged;
					if(notifySourcePropertyChanged != null)
						notifySourcePropertyChanged.PropertyChanged -= SourceObject_PropertyChanged;

					INotifyPropertyChanged	notifyDestinationPropertyChanged	= DestinationObject as INotifyPropertyChanged;
					if(notifyDestinationPropertyChanged != null)
						notifyDestinationPropertyChanged.PropertyChanged -= DestinationObject_PropertyChanged;
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
		public string SourcePropertyName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property info used to get or set values from the source property.
		/// </summary>
		protected PropertyInfo SourcePropertyInfo
		{
			get;
			set;
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
		public string DestinationPropertyName
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property info used to get or set values from the destination property.
		/// </summary>
		protected PropertyInfo DestinationPropertyInfo
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates if the source property's Get accessor exists and is public.
		/// </summary>
		protected bool SourcePropertyCanGet
		{
			get;
			set;
		}

		/// <summary>
		/// Indicates if the destination property's Get accessor exists and is public.
		/// </summary>
		protected bool DestinationPropertyCanGet
		{
			get;
			set;
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
		#endregion

		#region Event handlers
		/// <summary>
		/// Copy the source property value to the destination property, when the source property changes.
		/// </summary>
		/// <param name="sender">Source object on which the property changed.</param>
		/// <param name="e">Name of the property that changed.</param>
		protected virtual void SourceObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			lock(LockObject)
			{
				if(Disposed)
					return;

				if(e.PropertyName != SourcePropertyName)
					return;

				UpdateDestinationProperty();
			}
		}

		/// <summary>
		/// Copy the destination property value to the source property, when the destination property changes.
		/// </summary>
		/// <param name="sender">Destination object on which the property changed.</param>
		/// <param name="e">Name of the property that changed.</param>
		protected virtual void DestinationObject_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			lock(LockObject)
			{
				if(Disposed)
					return;

				if(e.PropertyName != DestinationPropertyName)
					return;

				if(BindingMode != BindingModes.TwoWay)
					return;

				UpdateSourceProperty();
			}
		}
		#endregion

		#region Methods
		/// <summary>
		/// Verify that the properties, needed for setting up the binding, can be accessed and then create event subscriptions
		/// to listen to property changes.
		/// </summary>
		protected virtual void Initialize()
		{
			lock(LockObject)
			{
				if(Disposed)
					throw new ObjectDisposedException(nameof(Binding));

				Type					sourceObjectType				= SourceObject.GetType();
				PropertyInfo			sourcePropertyInfo				= sourceObjectType.GetProperty(SourcePropertyName);
				Type					sourcePropertyType				= sourcePropertyInfo.GetType();
				MethodInfo				sourcePropertyGetAccessor		= sourcePropertyInfo.GetGetMethod();
				INotifyPropertyChanged	notifySourcePropertyChanged		= SourceObject as INotifyPropertyChanged;

				Type			destinationObjectType			= DestinationObject.GetType();
				PropertyInfo	destinationPropertyInfo			= destinationObjectType.GetProperty(DestinationPropertyName);
				Type			destinationPropertyType			= destinationPropertyInfo.GetType();
				MethodInfo		destinationPropertySetAccessor	= destinationPropertyInfo.GetSetMethod();

				// Check that the source property can be read.
				if(!sourcePropertyGetAccessor.IsPublic)
					throw new ArgumentException($"The get accessor, of the source property \"{SourcePropertyName}\", isn't public.", SourcePropertyName);

				// Check that the destination property can be set.
				if(!destinationPropertySetAccessor.IsPublic)
					throw new ArgumentException($"The set accessor, of the destination property \"{DestinationPropertyName}\", isn't public.", DestinationPropertyName);

				// Check that the destination property can be set from the source property.
				bool destinationPropertyAssignable	= destinationPropertyType.IsAssignableFrom(sourcePropertyType);
				if(!destinationPropertyAssignable)
					throw new ArgumentException($"The destination property \"{DestinationPropertyName}\" is of type \"{destinationPropertyType.Name}\", and can't be assigned from the source property \"{SourcePropertyName}\" of type \"{sourcePropertyType.Name}\".", DestinationPropertyName);

				// Check that we can subscribe to change events from the source object.
				if(notifySourcePropertyChanged == null)
					throw new ArgumentException($"The source object doesn't implement the {nameof(INotifyPropertyChanged)} interface.", nameof(SourceObject));

				if(BindingMode == BindingModes.TwoWay)
				{
					MethodInfo				sourcePropertySetAccessor			= sourcePropertyInfo.GetSetMethod();
					MethodInfo				destinationPropertyGetAccessor		= destinationPropertyInfo.GetGetMethod();
					INotifyPropertyChanged	notifyDestinationPropertyChanged	= DestinationObject as INotifyPropertyChanged;

					// Check that the destination property can be read, for two way bindings.
					if(!destinationPropertyGetAccessor.IsPublic)
						throw new ArgumentException($"The get accessor, of the destination property \"{DestinationPropertyName}\", isn't public.", DestinationPropertyName);

					// Check that the source property can be set, for two way bindings.
					if(!sourcePropertySetAccessor.IsPublic)
						throw new ArgumentException($"The set accessor, of the source property \"{SourcePropertyName}\", isn't public.", SourcePropertyName);

					// Check that the source property can be set from the destination property.
					bool sourcePropertyAssignable	= sourcePropertyType.IsAssignableFrom(destinationPropertyType);
					if(!sourcePropertyAssignable)
						throw new ArgumentException($"The source property \"{SourcePropertyName}\" is of type \"{sourcePropertyType.Name}\", and can't be assigned from the destination property \"{DestinationPropertyName}\" of type \"{destinationPropertyType.Name}\".", SourcePropertyName);

					// Check that we can subscribe to change events from the destination object.
					if(notifyDestinationPropertyChanged == null)
						throw new ArgumentException($"The destination object doesn't implement the {nameof(INotifyPropertyChanged)} interface.", nameof(DestinationObject));

					DestinationPropertyCanGet							= destinationPropertyGetAccessor != null && destinationPropertyGetAccessor.IsPublic;
					notifyDestinationPropertyChanged.PropertyChanged    += DestinationObject_PropertyChanged;
				}

				SourcePropertyInfo								= sourcePropertyInfo;
				DestinationPropertyInfo							= destinationPropertyInfo;
				SourcePropertyCanGet							= sourcePropertyGetAccessor != null && sourcePropertyGetAccessor.IsPublic;
				notifySourcePropertyChanged.PropertyChanged		+= SourceObject_PropertyChanged;
			}
		}

		/// <summary>
		/// Copy the value from the source property to the destination property.
		/// </summary>
		protected virtual void UpdateDestinationProperty()
		{
			lock(LockObject)
			{
				if(Disposed)
					throw new ObjectDisposedException(nameof(Binding));

				try
				{
					object sourcePropertyValue = SourcePropertyInfo.GetValue(SourceObject);

					if(DestinationPropertyCanGet)
					{
						// If the destination value is the same as the source value, don't update the destination property.
						object destinationPropertyValue = DestinationPropertyInfo.GetValue(DestinationObject);
						if(sourcePropertyValue.Equals(destinationPropertyValue))
							return;
					}

					DestinationPropertyInfo.SetValue(DestinationObject, sourcePropertyValue);
				}
				catch
				{
				}
			}
		}

		/// <summary>
		/// Copy the value from the destination property to the source property.
		/// </summary>
		protected virtual void UpdateSourceProperty()
		{
			lock(LockObject)
			{
				if(Disposed)
					throw new ObjectDisposedException(nameof(Binding));

				if(BindingMode != BindingModes.TwoWay)
					return;

				try
				{
					object destinationPropertyValue = DestinationPropertyInfo.GetValue(DestinationObject);

					if(SourcePropertyCanGet)
					{
						// If the source value is the same as the destination value, don't update the source property.
						object sourcePropertyValue = SourcePropertyInfo.GetValue(SourceObject);
						if(destinationPropertyValue.Equals(sourcePropertyValue))
							return;
					}

					SourcePropertyInfo.SetValue(SourceObject, destinationPropertyValue);
				}
				catch
				{
				}
			}
		}
		#endregion
	}
}
