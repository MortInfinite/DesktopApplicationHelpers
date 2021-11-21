using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace WpfHelpers
{
	public interface IAsyncCommand	:	ICommandHandler, 
										INotifyPropertyChanged
	{
		/// <summary>
		/// Token source, used to cancel the running task.
		/// </summary>
		CancellationTokenSource CancellationTokenSource
		{
			get;
		}

		/// <summary>
		/// Indicates if the command has started, but not yet completed.
		/// </summary>
		bool Running
		{
			get;
		}

		/// <summary>
		/// Indicates if the previously executed task has completed.
		/// </summary>
		bool Completed
		{
			get;
		}

		/// <summary>
		/// Indicates if the task has started or completed.
		/// </summary>
		bool RunningOrCompleted
		{
			get;
		}

		/// <summary>
		/// Indicates if the previously executed task has failed.
		/// </summary>
		bool Failed
		{
			get;
		}

		/// <summary>
		/// Exception thrown by the previously executed task.
		/// </summary>
		Exception Exception
		{
			get;
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		Task ExecuteAsync(object parameter);
	}
	/*
	public interface IAsyncCommand<T> :ICommand,
											INotifyPropertyChanged
	{
		/// <summary>
		/// Token source, used to cancel the running task.
		/// </summary>
		CancellationTokenSource CancellationTokenSource
		{
			get;
		}

		/// <summary>
		/// Result returned from the command.
		/// </summary>
		T Result
		{
			get;
		}

		/// <summary>
		/// Indicates if the command has started, but not yet completed.
		/// </summary>
		bool Running
		{
			get;
		}

		/// <summary>
		/// Indicates if the previously executed task has completed.
		/// </summary>
		bool Completed
		{
			get;
		}

		/// <summary>
		/// Indicates if the previously executed task has failed.
		/// </summary>
		bool Failed
		{
			get;
		}

		/// <summary>
		/// Exception thrown by the previously executed task.
		/// </summary>
		Exception Exception
		{
			get;
		}

		/// <summary>
		/// Defines the method to be called when the command is invoked.
		/// </summary>
		/// <param name="parameter">
		/// Data used by the command. If the command does not require data to be passed,
		/// this object can be set to null.
		/// </param>
		Task ExecuteAsync(object parameter);
	}
	*/
}