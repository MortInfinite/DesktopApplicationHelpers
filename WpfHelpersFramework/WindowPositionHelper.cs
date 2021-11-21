using System;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using Microsoft.Win32;

namespace WpfHelpers
{
	/// <summary>
	/// Provides functionality for storing and restoring of a Window's position, using the registry.
	/// </summary>
	/// <remarks>
	/// Based on code from:
	/// https://engy.us/blog/2010/03/08/saving-window-size-and-location-in-wpf-and-winforms/
	/// </remarks>
	public static class WindowPositionHelper
	{
		#region Types
		// RECT structure required by WINDOWPLACEMENT structure
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		protected struct RECT
		{
			public int Left;
			public int Top;
			public int Right;
			public int Bottom;

			public RECT(int left, int top, int right, int bottom)
			{
				this.Left = left;
				this.Top = top;
				this.Right = right;
				this.Bottom = bottom;
			}
		}

		// POINT structure required by WINDOWPLACEMENT structure
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		protected struct POINT
		{
			public int X;
			public int Y;

			public POINT(int x, int y)
			{
				this.X = x;
				this.Y = y;
			}
		}

		// WINDOWPLACEMENT stores the position, size, and state of a window
		[Serializable]
		[StructLayout(LayoutKind.Sequential)]
		protected struct WINDOWPLACEMENT
		{
			public int length;
			public int flags;
			public int showCmd;
			public POINT minPosition;
			public POINT maxPosition;
			public RECT normalPosition;
		}
		#endregion

		#region External methods
		[DllImport("user32.dll")]
		private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WINDOWPLACEMENT lpwndpl);

		[DllImport("user32.dll")]
		private static extern bool GetWindowPlacement(IntPtr hWnd, out WINDOWPLACEMENT lpwndpl);

		private const int SW_SHOWNORMAL = 1;
		private const int SW_SHOWMINIMIZED = 2;
		#endregion

		#region Methods
		/// <summary>
		/// Retrieve the version information from the executing assembly.
		/// </summary>
		/// <returns>Version info retrieved from the executing assembly or null if no version info was found.</returns>
		public static FileVersionInfo GetAssemblyVersionInfo()
		{
			string assemblyPath = Assembly.GetEntryAssembly()?.Location;
			if(string.IsNullOrEmpty(assemblyPath))
				return null;

			FileVersionInfo versionInfo	= FileVersionInfo.GetVersionInfo(assemblyPath);

			return versionInfo;
		}

		/// <summary>
		/// Stores the current window position in the registry, under the company and application name
		/// found in the assembly information.
		/// </summary>
		/// <param name="window">Window for which to store the position.</param>
		/// <param name="windowName">Optional name of the window to restore.</param>
		/// <returns>Returns true if the window position was successfully stored or false otherwise.</returns>
		/// <remarks>
		/// Call this method in the Window's OnSourceInitialized event handler.
		/// </remarks>
		public static bool StoreWindowPlacement(Window window, string windowName=null)
		{
			Assembly	assembly		= Assembly.GetEntryAssembly();
			string		applicationName	= assembly?.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			string		companyName		= assembly?.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

			if(string.IsNullOrEmpty(applicationName) || string.IsNullOrEmpty(companyName))
				return false;

			bool result = StoreWindowPlacement(window, companyName, applicationName, windowName);
			return result;
		}

		/// <summary>
		/// Stores the current window position in the registry.
		/// </summary>
		/// <param name="window">Window for which to store the position.</param>
		/// <param name="companyName">Name of the company who created the application.</param>
		/// <param name="applicationName">Name of the application for which to store the window position.</param>
		/// <param name="windowName">Optional name of the window to restore.</param>
		/// <returns>Returns true if the window position was successfully stored or false otherwise.</returns>
		/// <remarks>
		/// Call this method in the Window's OnSourceInitialized event handler.
		/// </remarks>
		public static bool StoreWindowPlacement(Window window, string companyName, string applicationName, string windowName=null)
		{
			if(window == null || string.IsNullOrEmpty(companyName) || string.IsNullOrEmpty(applicationName))
				return false;

			string registryKey;

			if(string.IsNullOrEmpty(windowName))
				registryKey = $"Software\\{companyName}\\{applicationName}\\WindowPosition";
			else
				registryKey = $"Software\\{companyName}\\{applicationName}\\{windowName}\\WindowPosition";

			try
			{
				WINDOWPLACEMENT windowPlacement;

				bool success = GetWindowPlacement(new WindowInteropHelper(window).Handle, out windowPlacement);
				if(!success)
					return false;

				using(RegistryKey key = Registry.CurrentUser.CreateSubKey(registryKey, true))
				{
					if(key == null)
						return false;

					key.SetValue("ShowCommand",				windowPlacement.showCmd,				RegistryValueKind.DWord);
					key.SetValue("MinPositionX",			windowPlacement.minPosition.X,			RegistryValueKind.DWord);
					key.SetValue("MinPositionY",			windowPlacement.minPosition.Y,			RegistryValueKind.DWord);
					key.SetValue("MaxPositionX",			windowPlacement.maxPosition.X,			RegistryValueKind.DWord);
					key.SetValue("MaxPositionY",			windowPlacement.maxPosition.Y,			RegistryValueKind.DWord);
					key.SetValue("NormalPositionLeft",		windowPlacement.normalPosition.Left,	RegistryValueKind.DWord);
					key.SetValue("NormalPositionTop",		windowPlacement.normalPosition.Top,		RegistryValueKind.DWord);
					key.SetValue("NormalPositionRight",		windowPlacement.normalPosition.Right,	RegistryValueKind.DWord);
					key.SetValue("NormalPositionBottom",	windowPlacement.normalPosition.Bottom,	RegistryValueKind.DWord);
				}
			}
			catch
			{
				return false;
			}

			return true;
		}

		/// <summary>
		/// Restores the window position from values retrived from the registry, under the company and application name
		/// found in the assembly information.
		/// </summary>
		/// <param name="window">Window for which to restore the position.</param>
		/// <param name="windowName">Optional name of the window to restore.</param>
		/// <returns>Returns true if the window position was successfully restored or false otherwise.</returns>
		/// <remarks>
		/// Call this method in the Window's OnClosing event handler (NOT in the OnClosed event handler).
		/// </remarks>
		public static bool RestoreWindowPlacement(Window window, string windowName=null)
		{
			Assembly	assembly		= Assembly.GetEntryAssembly();
			string		applicationName	= assembly?.GetCustomAttribute<AssemblyTitleAttribute>().Title;
			string		companyName		= assembly?.GetCustomAttribute<AssemblyCompanyAttribute>().Company;

			if(string.IsNullOrEmpty(applicationName) || string.IsNullOrEmpty(companyName))
				return false;

			bool result = RestoreWindowPlacement(window, companyName, applicationName, windowName);
			return result;
		}

		/// <summary>
		/// Restores the window position from values retrived from the registry.
		/// </summary>
		/// <param name="window">Window for which to restore the position.</param>
		/// <param name="companyName">Name of the company who created the application.</param>
		/// <param name="applicationName">Name of the application for which to restore the window position.</param>
		/// <param name="windowName">Optional name of the window to restore.</param>
		/// <returns>Returns true if the window position was successfully restored or false otherwise.</returns>
		/// <remarks>
		/// Call this method in the Window's OnClosing event handler (NOT in the OnClosed event handler).
		/// </remarks>
		public static bool RestoreWindowPlacement(Window window, string companyName, string applicationName, string windowName=null)
		{
			if(window == null || string.IsNullOrEmpty(applicationName))
				return false;

			string registryKey;

			if(string.IsNullOrEmpty(windowName))
				registryKey = $"Software\\Company name\\{applicationName}\\WindowPosition";
			else
				registryKey = $"Software\\Company name\\{applicationName}\\{windowName}\\WindowPosition";

			try
			{
				WINDOWPLACEMENT windowPlacement = new WINDOWPLACEMENT();

				using(RegistryKey key = Registry.CurrentUser.OpenSubKey(registryKey, false))
				{
					if(key == null)
						return false;

					windowPlacement.showCmd					= (int) key.GetValue("ShowCommand");
					windowPlacement.minPosition.X			= (int) key.GetValue("MinPositionX");
					windowPlacement.minPosition.Y			= (int) key.GetValue("MinPositionY");
					windowPlacement.maxPosition.X			= (int) key.GetValue("MaxPositionX");
					windowPlacement.maxPosition.Y			= (int) key.GetValue("MaxPositionY");
					windowPlacement.normalPosition.Left		= (int) key.GetValue("NormalPositionLeft");
					windowPlacement.normalPosition.Top		= (int) key.GetValue("NormalPositionTop");
					windowPlacement.normalPosition.Right	= (int) key.GetValue("NormalPositionRight");
					windowPlacement.normalPosition.Bottom	= (int) key.GetValue("NormalPositionBottom");
				}

				windowPlacement.length	= Marshal.SizeOf(typeof(WINDOWPLACEMENT));
				windowPlacement.flags	= 0;
				windowPlacement.showCmd = (windowPlacement.showCmd == SW_SHOWMINIMIZED ? SW_SHOWNORMAL : windowPlacement.showCmd);

				bool success = SetWindowPlacement(new WindowInteropHelper(window).Handle, ref windowPlacement);
				return success;
			}
			catch
			{
				return false;
			}
		}
		#endregion
	}
}
