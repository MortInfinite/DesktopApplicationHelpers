using System;
using System.Threading;

namespace WpfHelpers
{
	/// <summary>
	/// Provides extension methods for <see cref="SynchronizationContext"/>.
	/// </summary>
	public static class SynchronizationContextExtensions
	{
		/// <summary>
		/// Uses the synchronization context to send the specified sendOrPostCallback, if the current synchronization context 
		/// doesn't match the specified context. 
		/// 
		/// Otherwise calls the sendOrPostCallback on the current synchronization context.
		/// </summary>
		/// <param name="context">Context used to send the sendOrPostCallback.</param>
		/// <param name="action">Delegate to execute.</param>
		/// <param name="commandParameter">State to pass to the sendOrPostCallback delegate.</param>
		public static void SendIfNecessary(this SynchronizationContext context, Action action, object commandParameter = null)
		{
			SendIfNecessary(context, (unused) => action(), commandParameter);
		}

		/// <summary>
		/// Uses the synchronization context to send the specified sendOrPostCallback, if the current synchronization context 
		/// doesn't match the specified context. 
		/// 
		/// Otherwise calls the sendOrPostCallback on the current synchronization context.
		/// </summary>
		/// <param name="context">Context used to send the sendOrPostCallback.</param>
		/// <param name="sendOrPostCallback">Delegate to execute.</param>
		/// <param name="commandParameter">State to pass to the sendOrPostCallback delegate.</param>
		public static void SendIfNecessary(this SynchronizationContext context, SendOrPostCallback sendOrPostCallback, object commandParameter = null)
		{ 
			if(SynchronizationContext.Current != context)
				context.Send((state) => sendOrPostCallback(state), commandParameter);
			else
				sendOrPostCallback(commandParameter);
		}

		/// <summary>
		/// Uses the synchronization context to asynchronously post the specified sendOrPostCallback, if the current synchronization context 
		/// doesn't match the specified context. 
		/// 
		/// Otherwise calls the sendOrPostCallback synchronously on the current synchronization context.
		/// </summary>
		/// <param name="context">Context used to send the sendOrPostCallback.</param>
		/// <param name="action">Delegate to execute.</param>
		/// <param name="commandParameter">State to pass to the sendOrPostCallback delegate.</param>
		public static void PostIfNecessary(this SynchronizationContext context, Action action, object commandParameter = null)
		{
			PostIfNecessary(context, (unused) => action(), commandParameter);
		}

		/// <summary>
		/// Uses the synchronization context to asynchronously post the specified sendOrPostCallback, if the current synchronization context 
		/// doesn't match the specified context. 
		/// 
		/// Otherwise calls the sendOrPostCallback synchronously on the current synchronization context.
		/// </summary>
		/// <param name="context">Context used to send the sendOrPostCallback.</param>
		/// <param name="sendOrPostCallback">Delegate to execute.</param>
		/// <param name="commandParameter">State to pass to the sendOrPostCallback delegate.</param>
		public static void PostIfNecessary(this SynchronizationContext context, SendOrPostCallback sendOrPostCallback, object commandParameter = null)
		{
			if(SynchronizationContext.Current != context)
				context.Post((state) => sendOrPostCallback(state), commandParameter);
			else
				sendOrPostCallback(commandParameter);
		}
	}
}
