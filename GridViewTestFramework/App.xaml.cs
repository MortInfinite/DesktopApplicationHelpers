using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace GridViewTest
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App :Application
	{
		protected override void OnStartup(StartupEventArgs e)
		{
			base.OnStartup(e);

			DualListWindow dualListWindow = new DualListWindow();
			dualListWindow.Show();

			ConcurrentObservableCollectionWindow concurrentObservableCollectionWindow = new ConcurrentObservableCollectionWindow();
			concurrentObservableCollectionWindow.Show();

			FilteredListWindow filteredListWindow = new FilteredListWindow();
			filteredListWindow.Show();

			InvokeValueMarkupWindow invokeValueMarkupWindow = new InvokeValueMarkupWindow();
			invokeValueMarkupWindow.Show();

			MvvmWindow mvvmWindow	= new MvvmWindow()
			{
				DataContext = new MvvmViewModel()
			};

			mvvmWindow.Show();
		}
	}
}
