using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Input;

namespace WpfHelpers
{
	/// <summary>
	/// Exposes the CanExecute and Execute delegates of an ICommand, allowing the user of this class to 
	/// specify which delegates to use to implement the CanExecute and Execute methods.
	/// </summary>
	public class CommandHandler:ICommandHandler
	{
		/// <summary>
		/// Creates a new command handler.
		/// </summary>
		/// <param name="action">Action to execute, when the command is called or null to not execute anything.</param>
		/// <param name="canExecute">
		/// Function to call to determine if the action can be executed or null to always indicate 
		/// that the command can be executed.
		/// </param>
		/// <param name="context">
		/// Synchronization context on which notifications will be raised.
		/// If this is null, the current synchronization context will be used.
		/// </param>
		public CommandHandler(ExecuteDelegate action, CanExecuteDelegate canExecute=null, SynchronizationContext context=null)
		{
			if(context != null)
			{
				Context = context;
			}
			else
			{
				if(SynchronizationContext.Current == null)
					throw new InvalidOperationException($"The {GetType().Name} must be created on the UI thread.");

				Context     = SynchronizationContext.Current;
			}

			ActionImplementation        = action;
			CanExecuteImplementation    = canExecute;
		}

		protected CommandHandler()
		{
		}

		#region ICommand implementation
		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		/// <returns>true if this command can be executed; otherwise, false.</returns>
		[DebuggerStepThrough]
		public bool CanExecute(object parameter)
		{
			if(CanExecuteImplementation == null)
				return true;

			bool result = false;

			Context.SendIfNecessary((unused)=>
			{
				result = CanExecuteImplementation(parameter);
			}, null);

			return result;
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		[DebuggerStepThrough]
		public void Execute(object parameter)
		{
			// Check if the command can be executed.
			if(!CanExecute(parameter))
				return;

			if(ActionImplementation == null)
				return;

			Context.SendIfNecessary((unused)=>ActionImplementation.Invoke(parameter), null);
		}
		#endregion

		#region Type definitions
		/// <summary>
		/// Delegate used to execute a command.
		/// </summary>
		/// <param name="parameter">Parameter to pass to the command.</param>
		public delegate void ExecuteDelegate(object parameter);

		/// <summary>
		/// Delegate used to determine if a command can be executed.
		/// </summary>
		/// <param name="parameter">Parameter to pass to the command.</param>
		/// <returns>Returns true if the command can be executed.</returns>
		public delegate bool CanExecuteDelegate(object parameter);
		#endregion

		#region Methods
		/// <summary>
		/// Notifies subscribers that the CanExecute method parameters have changed.
		/// </summary>
		public void NotifyCanExecuteChanged()
		{
			Context.Post((unused)=>CanExecuteChanged?.Invoke(this, new EventArgs()), null);
		}
		#endregion

		#region Properties
		protected virtual ExecuteDelegate ActionImplementation
		{
			get;
			set;
		}

		protected virtual CanExecuteDelegate CanExecuteImplementation
		{
			get;
			set;
		}
		protected virtual SynchronizationContext Context
		{
			get;
			set;
		}
		#endregion
	}

	/// <summary>
	/// Exposes the CanExecute and Execute delegates of an ICommand, allowing the user of this class to 
	/// specify which delegates to use to implement the CanExecute and Execute methods.
	/// 
	/// Also specifies one additional parameter to pass to the delegates being called, when a command is executed.
	/// Additional parameters are specified when the command is constructed.
	/// </summary>
	public class CommandHandler<T1>	:ICommand
	{
		/// <summary>
		/// Creates a new command handler.
		/// </summary>
		/// <param name="action">Action to execute, when the command is called or null to not execute anything.</param>
		/// <param name="canExecute">
		/// Function to call to determine if the action can be executed or null to always indicate 
		/// that the command can be executed.
		/// </param>
		/// <param name="parameter1">Parameter to pass to the command being executed.</param>
		public CommandHandler(ExecuteDelegate action, CanExecuteDelegate canExecute=null, T1 parameter1=default(T1))
		{
			m_action		= action;
			m_canExecute	= canExecute;
			m_parameter1	= parameter1;
		}

		#region ICommand implementation
		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		/// <returns>true if this command can be executed; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			if(m_canExecute == null)
				return true;

			return m_canExecute(parameter, m_parameter1);
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		public void Execute(object parameter)
		{
			// Check if the command can be executed.
			if(!CanExecute(parameter))
				return;

			m_action?.Invoke(parameter, m_parameter1);
		}
		#endregion

		#region Type definitions
		/// <summary>
		/// Delegate used to execute a command.
		/// </summary>
		/// <param name="parameter">Parameter passed to the command, from the command caller.</param>
		/// <param name="parameter1">Parameter specified when this command was defined.</param>
		public delegate void ExecuteDelegate(object parameter, T1 parameter1);

		/// <summary>
		/// Delegate used to determine if a command can be executed.
		/// </summary>
		/// <param name="parameter">Parameter passed to the command, from the command caller.</param>
		/// <param name="parameter1">Parameter specified when this command was defined.</param>
		/// <returns>Returns true if the command can be executed.</returns>
		public delegate bool CanExecuteDelegate(object parameter, T1 parameter1);
		#endregion

		#region Methods
		/// <summary>
		/// Notifies subscribers that the CanExecute method parameters have changed.
		/// </summary>
		public void NotifyCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, new EventArgs());
		}
		#endregion

		#region Fields
		private ExecuteDelegate m_action;
		private CanExecuteDelegate m_canExecute;
		private T1 m_parameter1;
		#endregion
	}

	/// <summary>
	/// Exposes the CanExecute and Execute delegates of an ICommand, allowing the user of this class to 
	/// specify which delegates to use to implement the CanExecute and Execute methods.
	/// 
	/// Also specifies two additional parameters to pass to the delegates being called, when a command is executed.
	/// Additional parameters are specified when the command is constructed.
	/// </summary>
	public class CommandHandler<T1, T2>	:ICommand
	{
		/// <summary>
		/// Creates a new command handler.
		/// </summary>
		/// <param name="action">Action to execute, when the command is called or null to not execute anything.</param>
		/// <param name="canExecute">
		/// Function to call to determine if the action can be executed or null to always indicate 
		/// that the command can be executed.
		/// </param>
		/// <param name="parameter1">Parameter to pass to the command being executed.</param>
		/// <param name="parameter2">Parameter to pass to the command being executed.</param>
		public CommandHandler(ExecuteDelegate action, CanExecuteDelegate canExecute=null, T1 parameter1=default(T1), T2 parameter2=default(T2))
		{
			m_action		= action;
			m_canExecute	= canExecute;
			m_parameter1	= parameter1;
			m_parameter2	= parameter2;
		}

		#region ICommand implementation
		/// <summary>
		/// Occurs when changes occur that affect whether or not the command should execute.
		/// </summary>
		public event EventHandler CanExecuteChanged;

		/// <summary>
		/// Defines the method that determines whether the command can execute in its current state.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		/// <returns>true if this command can be executed; otherwise, false.</returns>
		public bool CanExecute(object parameter)
		{
			if(m_canExecute == null)
				return true;

			return m_canExecute(parameter, m_parameter1, m_parameter2);
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		public void Execute(object parameter)
		{
			// Check if the command can be executed.
			if(!CanExecute(parameter))
				return;

			m_action?.Invoke(parameter, m_parameter1, m_parameter2);
		}
		#endregion

		#region Type definitions
		/// <summary>
		/// Delegate used to execute a command.
		/// </summary>
		/// <param name="parameter">Parameter passed to the command, from the command caller.</param>
		/// <param name="parameter1">Parameter specified when this command was defined.</param>
		/// <param name="parameter2">Parameter specified when this command was defined.</param>
		public delegate void ExecuteDelegate(object parameter, T1 parameter1, T2 parameter2);

		/// <summary>
		/// Delegate used to determine if a command can be executed.
		/// </summary>
		/// <param name="parameter">Parameter passed to the command, from the command caller.</param>
		/// <param name="parameter1">Parameter specified when this command was defined.</param>
		/// <param name="parameter2">Parameter specified when this command was defined.</param>
		/// <returns>Returns true if the command can be executed.</returns>
		public delegate bool CanExecuteDelegate(object parameter, T1 parameter1, T2 parameter2);
		#endregion

		#region Methods
		/// <summary>
		/// Notifies subscribers that the CanExecute method parameters have changed.
		/// </summary>
		public void NotifyCanExecuteChanged()
		{
			CanExecuteChanged?.Invoke(this, new EventArgs());
		}
		#endregion

		#region Fields
		private ExecuteDelegate m_action;
		private CanExecuteDelegate m_canExecute;
		private T1 m_parameter1;
		private T2 m_parameter2;
		#endregion
	}
}
