using System.Windows.Input;

namespace WpfHelpers
{
	/// <summary>
	/// Adds functionality to the ICommand interface, allowing the caller to notify subscribers
	/// that the CanExecute has changed.
	/// </summary>
	public interface ICommandHandler	:ICommand
	{
		/// <summary>
		/// Notifies subscribers that the CanExecute method parameters have changed.
		/// </summary>
		void NotifyCanExecuteChanged();
	}
}
