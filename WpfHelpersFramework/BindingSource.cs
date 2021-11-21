using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;

namespace WpfHelpers
{
	/// <summary>
	/// Delegate used to indicate that a dependency property's value has changed.
	/// </summary>
	/// <param name="name">Name of the dependency property.</param>
	/// <param name="value">New value of the dependency property.</param>
	public delegate void DependencyPropertyChangedDelegate(string name, object value);

	/// <summary>
	/// Listens to changes in a dependency property and calls a delegate or sets a data context property,
	/// when the dependency property changes.
	/// </summary>
	public class BindingSource	:IDisposable
	{
		#region Constructors
		/// <summary>
		/// Listens to changes in a dependency property and calls the propertyChanged delegate, when the value changes.
		/// </summary>
		/// <param name="dependencyProperty">Dependency property to listen to, for changes.</param>
		/// <param name="dependencyPropertyClass">Class containing the definition of the dependency property.</param>
		/// <param name="dependencyObject">Object containing the dependency property.</param>
		/// <param name="propertyChanged">Delegate to call, when the dependency property changes.</param>
		/// <example>
		/// <![CDATA[
		/// new BindingSource(FrameworkElement.ActualWidthProperty, typeof(FrameworkElement), MyCanvas, (string name, object value)=>{Console.WriteLine($"{name} = {value}"});
		/// new BindingSource(Canvas.LeftProperty, typeof(Canvas), MyControl, (string name, object value)=>{Console.WriteLine($"{name} = {value}"});
		/// ]]></example>
		public BindingSource(DependencyProperty dependencyProperty, Type dependencyPropertyClass, object dependencyObject, DependencyPropertyChangedDelegate propertyChanged)
		{
			Initialize(dependencyProperty, dependencyPropertyClass, dependencyObject, propertyChanged);
		}

		/// <summary>
		/// Listening to changes in a dependency property and sets the value on the data context property, 
		/// when the dependency object value changes.
		/// </summary>
		/// <param name="dataContextOwner">Object containing the DataContext on which to set the property value.</param>
		/// <param name="dataContextPropertyName">Name of the property to set, on the DataContext, when the dependency property changes.</param>
		/// <param name="dependencyProperty">Dependency property to listen to, for changes.</param>
		/// <param name="dependencyPropertyClass">Class containing the definition of the dependency property.</param>
		/// <param name="dependencyObject">Object containing the dependency property.</param>
		/// <example>
		/// <![CDATA[
		/// new BindingSource(this, "CanvasWidth", FrameworkElement.ActualWidthProperty, typeof(FrameworkElement), MyCanvas);
		/// new BindingSource(this, "MyControlLeft", Canvas.LeftProperty, typeof(Canvas), MyControl);
		/// ]]></example>
		public BindingSource(FrameworkElement dataContextOwner, string dataContextPropertyName, DependencyProperty dependencyProperty, Type dependencyPropertyClass, object dependencyObject)
		{
			if(dataContextOwner == null)
				throw new ArgumentNullException(nameof(dataContextOwner));
			if(string.IsNullOrEmpty(dataContextPropertyName))
				throw new ArgumentNullException(nameof(dataContextPropertyName));

			Initialize(dependencyProperty, dependencyPropertyClass, dependencyObject, (string name, object value)=>SetPropertyOnDataContext(dataContextOwner, dataContextPropertyName, value, true));
		}

		/// <summary>
		/// Starts listening to changes in a dependency property and calls the propertyChanged delegate, when the value changes.
		/// </summary>
		/// <param name="dependencyProperty">Dependency property to listen to, for changes.</param>
		/// <param name="dependencyPropertyClass">Class containing the definition of the dependency property.</param>
		/// <param name="dependencyObject">Object containing the dependency property.</param>
		/// <param name="propertyChanged">Delegate to call, when the dependency property changes.</param>
		protected void Initialize(DependencyProperty dependencyProperty, Type dependencyPropertyClass, object dependencyObject, DependencyPropertyChangedDelegate propertyChanged)
		{
			if(dependencyProperty == null)
				throw new ArgumentNullException(nameof(dependencyProperty));
			if(dependencyPropertyClass == null)
				throw new ArgumentNullException(nameof(dependencyPropertyClass));
			if(dependencyObject == null)
				throw new ArgumentNullException(nameof(dependencyObject));
			if(propertyChanged == null)
				throw new ArgumentNullException(nameof(propertyChanged));
			
			DependencyObject				= dependencyObject;
			DependencyPropertyChanged		= propertyChanged;
			Descriptor						= DependencyPropertyDescriptor.FromProperty(dependencyProperty, dependencyPropertyClass);
			Descriptor.AddValueChanged(DependencyObject, OnPropertyChanged);
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
			if(Disposed)
				return;

			// TODO: Dispose unmanaged code here.

			if(disposing)
			{
				Descriptor?.RemoveValueChanged(DependencyObject, OnPropertyChanged);
				Descriptor = null;
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

		#region Event handlers
		/// <summary>
		/// Calls the <see cref="DependencyPropertyChanged"/> delegate, when the dependency property changes.
		/// </summary>
		/// <param name="sender">Object on which the dependency property changed.</param>
		/// <param name="e">Not used.</param>
		protected virtual void OnPropertyChanged(object sender, EventArgs e)
		{
			// Retrieve the value of the dependency property.
			object value = Descriptor.GetValue(DependencyObject);

			DependencyPropertyChanged?.Invoke(Descriptor.Name, value);
		}
		#endregion

		#region Properties
		/// <summary>
		/// Delegate to call, when the dependency property changes.
		/// </summary>
		protected virtual DependencyPropertyChangedDelegate DependencyPropertyChanged
		{
			get;
			set;
		}

		/// <summary>
		/// Descriptor of the dependency property that is being observed.
		/// </summary>
		protected virtual DependencyPropertyDescriptor Descriptor
		{
			get;
			set;
		}

		/// <summary>
		/// Object containing the dependency property that is being observed.
		/// </summary>
		protected virtual object DependencyObject
		{
			get;
			set;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Find the property with the specified name, in the DataContext of this object, and set its value.
		/// </summary>
		/// <param name="frameworkElement">Object on which to find the DataContext.</param>
		/// <param name="propertyName">Name of the property to set the value of.</param>
		/// <param name="throwIfNotExists">
		/// When true, throws an exception if the property couldn't be retrieved. 
		/// When false, returns false, if the property couldn't be retrieved.
		/// </param>
		/// <returns>Returns true if successful or false if the property could not be set.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if throwIfNotExists is true and the property couldn't be set.</exception>
		protected bool SetPropertyOnDataContext(FrameworkElement frameworkElement, string propertyName, object value, bool throwIfNotExists=true)
		{
			// Check if the data context is valid.
			if(frameworkElement.DataContext == null)
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(frameworkElement.DataContext), $"The {nameof(frameworkElement.DataContext)} doesn't exist.");
				else
					return false;
			}

			// Get the data context type, such that a property can be retrieved from it.
			Type dataContextType = frameworkElement.DataContext.GetType();
			if(dataContextType == null)
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(frameworkElement.DataContext), $"The {nameof(frameworkElement.DataContext)} type couldn't be resolved.");
				else
					return false;
			}

			// Retrieve the property information from the data context.
			PropertyInfo propertyInfo = dataContextType.GetProperty(propertyName);
			if(propertyInfo == null)
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(frameworkElement.DataContext), $"The {propertyName} property doesn't exist on the {nameof(frameworkElement.DataContext)}.");
				else
					return false;
			}

			// Set the value of the property.
			propertyInfo.SetValue(frameworkElement.DataContext, value);

			return true;
		}
		#endregion

	}
}
