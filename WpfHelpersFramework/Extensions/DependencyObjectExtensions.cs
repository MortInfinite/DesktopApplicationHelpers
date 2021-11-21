using System.Windows;
using System.Windows.Data;

namespace WpfHelpers
{
	/// <summary>
	/// Provides extention methods for <see cref="DependencyObject"/>.
	/// </summary>
	public static class DependencyObjectExtensions
	{
		/// <summary>
		/// Indicates if this object is a child of the specified parent.
		/// </summary>
		/// <param name="thisObject">The object to test if is a child of the specified parent.</param>
		/// <param name="parent">Parent that this object must be a child of.</param>
		/// <returns>Returns true if this object is a child of the specified parent.</returns>
		public static bool IsChildOf(this DependencyObject thisObject, DependencyObject parent)
		{
			DependencyObject currentElement = thisObject;
			while(currentElement != null)
			{
				if(currentElement == parent)
					return true;

				currentElement = LogicalTreeHelper.GetParent(currentElement);
			}

			return false;
		}

		/// <summary>
		/// Creates a binding from the specified source property and source object to the specified destination property of this object.
		/// </summary>
		/// <param name="destinationObject">Object to bind to.</param>
		/// <param name="destinationProperty">Dependency property to bind to.</param>
		/// <param name="sourceObject">Object to bind from.</param>
		/// <param name="sourcePropertyName">Name of the property to bind from.</param>
		/// <param name="mode">Binding mode to use in the binding.</param>
		/// <param name="converter">Value converter to apply to the binding.</param>
		/// <returns>Created binding.</returns>
		public static System.Windows.Data.Binding SetBinding(this DependencyObject destinationObject, DependencyProperty destinationProperty, string sourcePropertyName, System.Windows.Data.BindingMode mode = System.Windows.Data.BindingMode.OneWay, IValueConverter converter = null)
		{
			System.Windows.Data.Binding binding = new System.Windows.Data.Binding(sourcePropertyName);
			binding.Mode                        = mode;
			binding.Converter                   = converter;

			System.Windows.Data.BindingOperations.SetBinding(destinationObject, destinationProperty, binding);
			return binding;
		}

		/// <summary>
		/// Creates a binding from the specified source property and source object to the specified destination property of this object.
		/// </summary>
		/// <param name="destinationObject">Object to bind to.</param>
		/// <param name="destinationProperty">Dependency property to bind to.</param>
		/// <param name="sourceObject">Object to bind from.</param>
		/// <param name="sourcePropertyName">Name of the property to bind from.</param>
		/// <param name="mode">Binding mode to use in the binding.</param>
		/// <param name="converter">Value converter to apply to the binding.</param>
		/// <param name="fallbackValue">Fallback value, to apply to the binding, or null to not apply a fallback value.</param>
		/// <returns>Created binding.</returns>
		public static System.Windows.Data.Binding SetBinding(this DependencyObject destinationObject, DependencyProperty destinationProperty, string sourcePropertyName, System.Windows.Data.BindingMode mode, IValueConverter converter, object fallbackValue)
		{
			System.Windows.Data.Binding binding	= new System.Windows.Data.Binding(sourcePropertyName);
			binding.Mode						= mode;
			binding.Converter					= converter;
			binding.FallbackValue               = fallbackValue;

			System.Windows.Data.BindingOperations.SetBinding(destinationObject, destinationProperty, binding);
			return binding;
		}

		/// <summary>
		/// Creates a binding from the specified source property and source object to the specified destination property of this object.
		/// </summary>
		/// <param name="destinationObject">Object to bind to.</param>
		/// <param name="destinationProperty">Dependency property to bind to.</param>
		/// <param name="sourceObject">Object to bind from.</param>
		/// <param name="sourcePropertyName">Name of the property to bind from.</param>
		/// <param name="mode">Binding mode to use in the binding.</param>
		/// <param name="converter">Value converter to apply to the binding.</param>
		/// <returns>Created binding.</returns>
		public static System.Windows.Data.Binding SetBinding(this DependencyObject destinationObject, DependencyProperty destinationProperty, object sourceObject, string sourcePropertyName, System.Windows.Data.BindingMode mode=System.Windows.Data.BindingMode.OneWay, IValueConverter converter=null)
		{
			System.Windows.Data.Binding binding	= new System.Windows.Data.Binding(sourcePropertyName);
			binding.Source						= sourceObject;
			binding.Mode						= mode;
			binding.Converter					= converter;

			System.Windows.Data.BindingOperations.SetBinding(destinationObject, destinationProperty, binding);
			return binding;
		}

		/// <summary>
		/// Creates a binding from the specified source property and source object to the specified destination property of this object.
		/// </summary>
		/// <param name="destinationObject">Object to bind to.</param>
		/// <param name="destinationProperty">Dependency property to bind to.</param>
		/// <param name="sourceObject">Object to bind from.</param>
		/// <param name="sourcePropertyName">Name of the property to bind from.</param>
		/// <param name="mode">Binding mode to use in the binding.</param>
		/// <param name="converter">Value converter to apply to the binding.</param>
		/// <param name="fallbackValue">Fallback value, to apply to the binding.</param>
		/// <returns>Created binding.</returns>
		public static System.Windows.Data.Binding SetBinding(this DependencyObject destinationObject, DependencyProperty destinationProperty, object sourceObject, string sourcePropertyName, System.Windows.Data.BindingMode mode, IValueConverter converter, object fallbackValue)
		{
			System.Windows.Data.Binding binding = new System.Windows.Data.Binding(sourcePropertyName);
			binding.Source                      = sourceObject;
			binding.Mode                        = mode;
			binding.Converter                   = converter;
			binding.FallbackValue               = fallbackValue;

			System.Windows.Data.BindingOperations.SetBinding(destinationObject, destinationProperty, binding);
			return binding;
		}
	}
}
