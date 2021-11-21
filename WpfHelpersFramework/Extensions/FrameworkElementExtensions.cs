using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace WpfHelpers
{
	/// <summary>
	/// Provides extension methods for <see cref="FrameworkElement"/>.
	/// </summary>
	public static class FrameworkElementExtensions
	{
		/// <summary>
		/// Find the command with the specified name, in the DataContext of this object, and execute the command.
		/// </summary>
		/// <param name="thisObject">Object on which to find the DataContext.</param>
		/// <param name="commandName">Name of the command to execute.</param>
		/// <param name="commandParameter">Parameter to pass to the command.</param>
		/// <returns>Returns true if the command was executed or false otherwise.</returns>
		public static bool ExecuteCommandFromDataContext(this FrameworkElement thisObject, string commandName, object commandParameter = null)
		{
			// Check if the data context is valid.
			if(thisObject.DataContext == null)
				return false;

			// Get the data context type, such that a property can be retrieved from it.
			Type dataContextType = thisObject.DataContext.GetType();
			if(dataContextType == null)
				return false;

			// Retrieve the command property information from the data context.
			PropertyInfo commandProperty = dataContextType.GetProperty(commandName);
			if(commandProperty == null)
				return false;

			// Retrieve the command property from the command property information.
			ICommand command = commandProperty.GetValue(thisObject.DataContext) as ICommand;
			if(command == null)
				return false;

			// Determine if the command can execute.
			if(!command.CanExecute(commandParameter))
				return false;

			// Execute the command.
			command.Execute(commandParameter);

			return true;
		}

		/// <summary>
		/// Find the property with the specified name, in the DataContext of this object, and retrieve its value.
		/// </summary>
		/// <param name="thisObject">Object on which to find the DataContext.</param>
		/// <param name="propertyName">Name of the property to retrieve the value of.</param>
		/// <param name="throwIfNotExists">
		/// When true, throws an exception if the property couldn't be retrieved. 
		/// When false, returns the default value of T, if the property couldn't be retrieved.
		/// </param>
		/// <returns>Returns value of the property or default(T) if the property couldn't be retrieved and throwIfNotExists is set to false.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if throwIfNotExists is true and the property couldn't be retrieved.</exception>
		public static T GetPropertyFromDataContext<T>(this FrameworkElement thisObject, string propertyName, bool throwIfNotExists=true)
		{
			// Check if the data context is valid.
			if(thisObject.DataContext == null)
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(thisObject.DataContext), $"The {nameof(thisObject.DataContext)} doesn't exist.");
				else
					return default(T);
			}

			// Get the data context type, such that a property can be retrieved from it.
			Type dataContextType = thisObject.DataContext.GetType();
			if(dataContextType == null)
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(thisObject.DataContext), $"The {nameof(thisObject.DataContext)} type couldn't be resolved.");
				else
					return default(T);
			}

			// Retrieve the property information from the data context.
			PropertyInfo propertyInfo = dataContextType.GetProperty(propertyName);
			if(propertyInfo == null)
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(thisObject.DataContext), $"The {nameof(thisObject.DataContext)} property doesn't exist.");
				else
					return default(T);
			}

			// Retrieve the value from the property.
			object retrievedUntypedValue = propertyInfo.GetValue(thisObject.DataContext);
			if(!(retrievedUntypedValue is T retrievedTypedValue))
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(T), $"The property {propertyName} wasn't of type {typeof(T)}.");
				else
					return default(T);
			}

			return retrievedTypedValue;
		}

		/// <summary>
		/// Find the property with the specified name, in the DataContext of this object, and set its value.
		/// </summary>
		/// <param name="thisObject">Object on which to find the DataContext.</param>
		/// <param name="propertyName">Name of the property to set the value of.</param>
		/// <param name="throwIfNotExists">
		/// When true, throws an exception if the property couldn't be retrieved. 
		/// When false, returns false, if the property couldn't be retrieved.
		/// </param>
		/// <returns>Returns true if successful or false if the property could not be set.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if throwIfNotExists is true and the property couldn't be set.</exception>
		public static bool SetPropertyOnDataContext<T>(this FrameworkElement thisObject, string propertyName, T value, bool throwIfNotExists=true)
		{
			// Check if the data context is valid.
			if(thisObject.DataContext == null)
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(thisObject.DataContext), $"The {nameof(thisObject.DataContext)} doesn't exist.");
				else
					return false;
			}

			// Get the data context type, such that a property can be retrieved from it.
			Type dataContextType = thisObject.DataContext.GetType();
			if(dataContextType == null)
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(thisObject.DataContext), $"The {nameof(thisObject.DataContext)} type couldn't be resolved.");
				else
					return false;
			}

			// Retrieve the property information from the data context.
			PropertyInfo propertyInfo = dataContextType.GetProperty(propertyName);
			if(propertyInfo == null)
			{
				if(throwIfNotExists)
					throw new ArgumentOutOfRangeException(nameof(thisObject.DataContext), $"The {propertyName} property doesn't exist on the {nameof(thisObject.DataContext)}.");
				else
					return false;
			}

			// Set the value of the property.
			propertyInfo.SetValue(thisObject.DataContext, value);

			return true;
		}

		/// <summary>
		/// Find the command with the specified name, in the DataContext of this object, and set its value to a new CommandHandler.
		/// </summary>
		/// <param name="thisObject">Object on which to find the DataContext.</param>
		/// <param name="commandName">Name of the command to set the value of.</param>
		/// <param name="context">SynchronizationContext used to execute the command or null to use the current SynchronizationContext.</param>
		/// <param name="throwIfNotExists">
		/// When true, throws an exception if the command couldn't be set. 
		/// When false, returns null, if the command couldn't be set.
		/// </param>
		/// <returns>Returns the created command or null if the command could not be set.</returns>
		/// <exception cref="ArgumentOutOfRangeException">Thrown if throwIfNotExists is true and the command couldn't be set.</exception>
		public static CommandHandler SetCommandOnDataContext(this FrameworkElement thisObject, string commandName, CommandHandler.ExecuteDelegate action, CommandHandler.CanExecuteDelegate canExecute=null, SynchronizationContext context=null, bool throwIfNotExists=true)
		{
			CommandHandler commandHandler = new CommandHandler(action, canExecute, context);

			bool success = SetPropertyOnDataContext<ICommand>(thisObject, commandName, commandHandler, throwIfNotExists);
			if(!success)
				return null;

			return commandHandler;
		}
	}
}
