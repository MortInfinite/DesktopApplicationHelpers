using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;

namespace WpfHelpers
{
	/// <summary>
	/// Exposes the a CanExecute and an asynchronous Execute delegate, used to execute an ICommand, allowing the user of this class to 
	/// specify which delegates to use to implement the CanExecute and Execute methods.
	/// </summary>
	public class AsyncCommandHandler :IAsyncCommand
	{
		/// <summary>
		/// Creates a new asynchronous command handler.
		/// </summary>
		/// <param name="action">Action to execute, when the command is called or null to not execute anything.</param>
		/// <param name="canExecute">
		/// Function to call to determine if the action can be executed or null to always indicate 
		/// that the command can be executed.
		/// </param>
		/// <param name="successAction">Action to execute when the executed task completes successfully, or null to not call anything on success.</param>
		/// <param name="failureAction">Action to execute when the executed task fails, or null to not call anything on failure.</param>
		public AsyncCommandHandler(ExecuteAsyncDelegate action, CanExecuteDelegate canExecute = null, ExecuteDelegate successAction = null, ExecuteDelegate failureAction = null, SynchronizationContext context = null)
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

			ActionImplementation            = action;
			CanExecuteImplementation        = canExecute;
			SuccessActionImplementation     = successAction;
			FailureActionImplementation     = failureAction;

			Context     = SynchronizationContext.Current;
		}

		protected AsyncCommandHandler()
		{
		}

		#region INotifyPropertyChanged
		/// <summary>
		/// Occurs when a property value changes.
		/// </summary>
		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Notify subscribers that a property changed.
		/// </summary>
		/// <param name="propertyName">Name of the property that changed.</param>
		protected virtual void NotifyPropertyChanged(string propertyName)
		{
			Context.SendIfNecessary(() => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName)));
		}

		/// <summary>
		/// Notify subscribers that a property changed.
		/// </summary>
		/// <param name="propertyChangedEventArgs">Information about the property that changed.</param>
		protected virtual void NotifyPropertyChanged(PropertyChangedEventArgs propertyChangedEventArgs)
		{
			Context.SendIfNecessary(() => PropertyChanged?.Invoke(this, propertyChangedEventArgs));
		}

		/// <summary>
		/// Notify subscribers that a property changed.
		/// </summary>
		/// <param name="sender">Reference to the object on which the property changed.</param>
		/// <param name="propertyChangedEventArgs">Information about the property that changed.</param>
		protected virtual void NotifyPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			Context.SendIfNecessary(() => PropertyChanged?.Invoke(this, e));
		}
		#endregion

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
			// Can't execute while already executing.
			if(Running)
				return false;

			if(CanExecuteImplementation == null)
				return true;

			bool result = false;

			Context.SendIfNecessary((unused) =>
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
		public void Execute(object parameter)
		{
			// Check if the command can be executed.
			if(!CanExecute(parameter))
				return;

			if(ActionImplementation == null)
				return;

			// Prepare for running the task.
			Exception   = null;
			Failed      = false;
			Completed   = false;

			Task task;
			CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
			try
			{
				// Start the task.
				task = ActionImplementation.Invoke(parameter, cancellationTokenSource.Token);
			}
			catch(Exception exception)
			{
				// If starting the task failed.
				Exception   = exception;
				Failed      = true;

				throw;
			}

			// If starting the task succeeded and the task is now running.
			Task                    = task;
			CancellationTokenSource = cancellationTokenSource;
			Running                 = true;

			// When the task completes, either successfully or by failing.
			task.ContinueWith((previousTask) =>
			{
				Running     = false;
				Completed   = true;
				Exception   = task.Exception?.InnerException;
				Failed      = task.IsFaulted;
			});
		}
		#endregion

		#region ICommandAsync implementation
		/// <summary>
		/// Token source, used to cancel the running task.
		/// </summary>
		public virtual CancellationTokenSource CancellationTokenSource
		{
			get
			{
				return m_cancellationTokenSource;
			}
			protected set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_cancellationTokenSource, value, NotifyPropertyChanged);
			}
		}

		/// <summary>
		/// Indicates if the command has started, but not yet completed.
		/// </summary>
		public virtual bool Running
		{
			get
			{
				return m_running;
			}
			protected set
			{
				// Update the field and notify subscribers that the property changed.
				bool modified = this.SetProperty(ref m_running, value, NotifyPropertyChanged);
				if(modified)
				{
					UpdateRunningOrCompleted();
					NotifyCanExecuteChanged();
				}
			}
		}

		/// <summary>
		/// Indicates if the previously executed task has completed.
		/// </summary>
		public virtual bool Completed
		{
			get
			{
				return m_complated;
			}
			protected set
			{
				// Update the field and notify subscribers that the property changed.
				bool modified = this.SetProperty(ref m_complated, value, NotifyPropertyChanged);
				if(modified)
					UpdateRunningOrCompleted();
			}
		}

		/// <summary>
		/// Indicates if the task has started or completed.
		/// </summary>
		public bool RunningOrCompleted
		{
			get
			{
				return m_runningOrCompleted;
			}
			protected set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_runningOrCompleted, value, NotifyPropertyChanged);
			}
		}

		/// <summary>
		/// Indicates if the previously executed task has failed.
		/// </summary>
		public bool Failed
		{
			get
			{
				return m_failed;
			}
			protected set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_failed, value, NotifyPropertyChanged);
			}
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		[DebuggerStepThrough]
		public Task ExecuteAsync(object parameter)
		{
			// Check if the command can be executed.
			if(!CanExecute(parameter))
				return Task.CompletedTask;

			if(ActionImplementation == null)
				return Task.CompletedTask;

			// Prepare for running the task.
			Exception   = null;
			Failed      = false;
			Completed   = false;

			CancellationTokenSource = new CancellationTokenSource();

			try
			{
				// Start the task.
				Task = ActionImplementation.Invoke(parameter, CancellationTokenSource.Token);

				// If starting the task succeeded and the task is now running.
				Running  = true;
			}
			catch(Exception exception)
			{
				// If starting the task failed.
				Exception   = exception.InnerException;
				Failed      = true;
				Running		= false;

				throw;
			}

			// When the task completes, either successfully or by failing.
			Task.ContinueWith((previousTask) =>
			{
				Running     = false;
				Completed   = true;
				Exception   = previousTask.Exception?.InnerException;
				Failed      = previousTask.IsFaulted;
			});

			return Task;
		}
		#endregion

		#region Type definitions
		/// <summary>
		/// Delegate used to execute a command.
		/// </summary>
		/// <param name="parameter">Parameter to pass to the command.</param>
		public delegate void ExecuteDelegate(object parameter);

		/// <summary>
		/// Delegate used to execute a command.
		/// </summary>
		/// <param name="parameter">Parameter to pass to the command.</param>
		/// <param name="cancellationToken">CancellationToken used to indicate that the task should abort.</param>
		public delegate Task ExecuteAsyncDelegate(object parameter, CancellationToken cancellationToken);

		/// <summary>
		/// Delegate used to determine if a command can be executed.
		/// </summary>
		/// <param name="parameter">Parameter to pass to the command.</param>
		/// <returns>Returns true if the command can be executed.</returns>
		public delegate bool CanExecuteDelegate(object parameter);
		#endregion

		#region Properties
		/// <summary>
		/// Exception thrown by the previously executed task.
		/// </summary>
		public Exception Exception
		{
			get
			{
				return m_exception;
			}
			protected set
			{
				// Update the field and notify subscribers that the property changed.
				this.SetProperty(ref m_exception, value, NotifyPropertyChanged);
			}
		}

		/// <summary>
		/// Asynchronous task being executed.
		/// </summary>
		protected virtual Task Task
		{
			get;
			set;
		}

		protected virtual ExecuteAsyncDelegate ActionImplementation
		{
			get;
			set;
		}

		protected virtual CanExecuteDelegate CanExecuteImplementation
		{
			get;
			set;
		}

		/// <summary>
		/// Action to execute when the executed task completes successfully.
		/// </summary>
		protected virtual ExecuteDelegate SuccessActionImplementation
		{
			get;
			set;
		}

		/// <summary>
		/// Action to execute when the executed task fails.
		/// </summary>
		protected virtual ExecuteDelegate FailureActionImplementation
		{
			get;
			set;
		}

		/// <summary>
		/// Synchronization context used to notify subscribers that a property has changed.
		/// </summary>
		protected virtual SynchronizationContext Context
		{
			get;
			set;
		}
		#endregion

		#region Methods
		/// <summary>
		/// Notifies subscribers that the CanExecute method parameters have changed.
		/// </summary>
		public void NotifyCanExecuteChanged()
		{
			Context.Post((unused) => CanExecuteChanged?.Invoke(this, new EventArgs()), null);
		}

		/// <summary>
		/// Updates the RunningOrCompleted property.
		/// </summary>
		[DebuggerStepThrough]
		protected virtual void UpdateRunningOrCompleted()
		{ 
			RunningOrCompleted = Running || Completed;
		}
		#endregion

		#region Fields
		/// <summary>
		/// Backing field for the CancellationTokenSource property.
		/// </summary>
		private CancellationTokenSource m_cancellationTokenSource;

		/// <summary>
		/// Backing field for the Running property.
		/// </summary>
		private bool m_running;

		/// <summary>
		/// Backing field for the Completed property.
		/// </summary>
		private bool m_complated;

		/// <summary>
		/// Backing field for the Exception property.
		/// </summary>
		private Exception m_exception;

		/// <summary>
		/// Backing field for the Failed property.
		/// </summary>
		private bool m_failed;

		/// <summary>
		/// Backing field for the RunningOrCompleted property.
		/// </summary>
		private bool m_runningOrCompleted;

		#endregion
	}
}
