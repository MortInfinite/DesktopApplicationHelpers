using System;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Markup;
using System.Xaml;

namespace WpfHelpers
{
	/// <summary>
	/// Markup extension that retrieves its value by calling a GetMethod delegate and sets its value by calling a SetMethod delegate.
	/// </summary>
	/// <example>
	/// <![CDATA[
	/// XAML usage:
	///	<CheckBox IsChecked="{local:InvokeValueMarkup GetMethod=GetIsChecked, SetMethod=SetIsChecked}"/>
	///		
	/// C# implementation:
	/// public object GetIsChecked(object dataContext)
	/// {
	/// 	return m_isChecked;
	/// }
	/// 
	/// public void SetIsChecked(object dataContext, object newValue)
	/// {
	/// 	m_isChecked = (bool) newValue;
	/// }
	/// ]]>
	/// </example>
    public class InvokeValueMarkup : MarkupExtension
    {
        #region Methods
		/// <summary>
		/// Returns an object that is provided as the value of the target property for this markup extension.
		/// </summary>
		/// <param name="serviceProvider">Service used to set the value on the target property.</param>
		/// <returns>The object value to set on the property where the extension is applied.</returns>
        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            FrameworkElement targetObject;

            // Attempt to retrieve the root object provider.
            // (This is only possible during WPF's initial call to this method, where the IProvideValueTarget 
            //  contains a SharedDp data type. Remember the RootObject value for use by the second call to this method)
            IRootObjectProvider rootObjectProvider = serviceProvider.GetService(typeof(IRootObjectProvider)) as IRootObjectProvider;
            if (rootObjectProvider != null)
                RootObject = rootObjectProvider.RootObject;

            IProvideValueTarget service = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;

            // If WPF is asking for the markup extention object to use to retrieve the markup extention value.
            if (service?.TargetObject?.GetType()?.FullName == "System.Windows.SharedDp")
                return this;

            targetObject = (FrameworkElement)service.TargetObject;
            TargetProperty = service.TargetProperty;

            if (TargetProperty is DependencyProperty)
            {
                DependencyProperty targetProperty = (DependencyProperty)TargetProperty;

                DependencyPropertyDescriptor dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(targetProperty, targetObject.GetType());
                dependencyPropertyDescriptor.AddValueChanged(targetObject, OnTargetPropertyValueChanged);
            }

            // Retrieve the value from either Name or Path properties.
            object retrievedValue = InvokeGetValue(targetObject.DataContext);

            return retrievedValue;
        }

        /// <summary>
        /// Call the SetMethod, to indicate that the value of the TargetProperty has changed.
        /// </summary>
        /// <param name="sender">Reference to the target property that changed.</param>
        /// <param name="e">Contains no data.</param>
        private void OnTargetPropertyValueChanged(object sender, EventArgs e)
        {
            DependencyProperty targetProperty = (DependencyProperty)TargetProperty;
            DependencyPropertyDescriptor dependencyPropertyDescriptor = DependencyPropertyDescriptor.FromProperty(targetProperty, sender.GetType());
            object newValue = dependencyPropertyDescriptor.GetValue(sender);
            object DataContext = null;

            // If the target object is a framework element, use the data context of the framework element, as the data source.
            FrameworkElement frameworkElement = sender as FrameworkElement;
            if (frameworkElement?.DataContext != null)
                DataContext = frameworkElement?.DataContext;

            // Invoke the Set method, indicating that the target property has changed.
            InvokeSetValue(DataContext, newValue);
        }

        /// <summary>
        /// Calls the GetValue method, on the TargetObject.
        /// </summary>
        /// <returns>Returns the retrieved value from the TargetObject.</returns>
        private object InvokeGetValue(object dataContext)
        {
            if (string.IsNullOrEmpty(GetMethod))
                return null;

            Type rootObjectType = RootObject?.GetType();
            MethodInfo methodInfo = rootObjectType?.GetMethod(GetMethod, new Type[] { typeof(object) });
            object result = methodInfo?.Invoke(RootObject, new object[] { dataContext });

            return result;
        }

        /// <summary>
        /// Calls the SetValue method, on the TargetObject.
        /// </summary>
        /// <param name="newValue">Value to specify as argument to the SetValue method.</param>
        private void InvokeSetValue(object dataContext, object newValue)
        {
            if (string.IsNullOrEmpty(SetMethod))
                return;

            Type rootObjectType = RootObject.GetType();
            MethodInfo methodInfo = rootObjectType?.GetMethod(SetMethod, new Type[] { typeof(object), typeof(object) });

            methodInfo?.Invoke(RootObject, new object[] { dataContext, newValue });
        }
        #endregion

        #region Properties
        /// <summary>
        /// Name of the method to call, to get a markup value.
        /// </summary>
        public string GetMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Name of the method to call, to set a markup value.
        /// </summary>
        public string SetMethod
        {
            get;
            set;
        }

        /// <summary>
        /// Root object containing all elements on the XAML page.
        /// </summary>
        private object RootObject
        {
            get;
            set;
        }

        /// <summary>
        /// Property that was provided by the previous call to ProvideValue.
        /// </summary>
        /// <remarks>
        /// This may contain a DependencyProperty or a PropertyInfo.
        /// </remarks>
        private object TargetProperty
        {
            get;
            set;
        }
        #endregion
    }
}
