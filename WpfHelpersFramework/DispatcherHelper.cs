using System;

namespace WpfHelpers
{
	public static class DispatcherHelper
	{
		/// <summary>
		/// Invoke the specified callback using a dispatcher, if not running on the UI thread or without
		/// a dispatcher if already running on the UI thread.
		/// </summary>
		/// <param name="dispatcher">Dispatcher to use to invoke the callback (If the dispatcher is needed).</param>
		/// <param name="callback">Action to execute.</param>
		public static void InvokeIfNeeded(this System.Windows.Threading.Dispatcher dispatcher, Action callback)
		{
			bool currentThreadHasAccess = System.Windows.Application.Current.CheckAccess();
			if(!currentThreadHasAccess)
				dispatcher.Invoke(callback);
			else
				callback();
		}

		/// <summary>
		/// Invoke the specified callback using a dispatcher, if not running on the UI thread or without
		/// a dispatcher if already running on the UI thread.
		/// </summary>
		/// <param name="dispatcher">Dispatcher to use to invoke the callback (If the dispatcher is needed).</param>
		/// <param name="function">Function to execute.</param>
		/// <returns>Returns the return value from the function.</returns>
		public static T InvokeIfNeeded<T>(this System.Windows.Threading.Dispatcher dispatcher, Func<T> function)
		{
			T result = default(T);

			bool currentThreadHasAccess = System.Windows.Application.Current.CheckAccess();
			if(!currentThreadHasAccess)
				dispatcher.Invoke(() => result = function());
			else
				result = function();

			return result;
		}
	}
}
