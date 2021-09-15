using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using CefSharp;
using CefSharp.Wpf;

namespace Canvas
{
	/// <summary>
	/// Interaction logic for MainWindow.xaml
	/// 
	/// V2.0
	/// enabele drag and drop
	/// - top menu
	/// |_File
	/// |	|_back
	/// |	|_forward
	/// |	|_reload
	/// |	|_home
	///	|	|--------------
	///	|	|_settings
	/// |	|--------------
	/// |	|_Exit
	/// |_Edit
	/// |_View
	///		|_[]keep menu bar
	///		|_Open in webbrowser
	///
	/// Context menu
	///	|_Back
	/// |_Forward
	///	|_Home
	/// ✓- catch pop up handle events
	/// - settings menu:
	///		[] keep signed in
	///		- username
	///		- password
	///		- style
	/// V3.0
	/// embed ico img
	/// CSS inject DarkMode
	/// body -> Backgroudn-color:#444
	/// .ToDoSidebarItem -> border-radius: 5px;
	///						padding:5px;
	///						Background-color:white;
	/// </summary>
	public partial class MainWindow : Window
	{
		Properties.Settings settings = Properties.Settings.Default;
		SettingsMenu settingsMenu = new SettingsMenu();
		const String BaseURL = "https://fhict.instructure.com/";
		public MainWindow()
		{
			WindowState = (WindowState)settings.WindowState;
			Height = settings.WindowHeight;
			Width = settings.WindowWidth;
			string cache_dir = Environment.CurrentDirectory + @"\cache";
			Cef.Initialize(new CefSettings() { CachePath = cache_dir });
			InitializeComponent();
			CefWeb.LifeSpanHandler = new LSHandler();
			CefWeb.DownloadHandler = (LSHandler)CefWeb.LifeSpanHandler;
			CefWeb.Address = BaseURL;
			CefWeb.Focus();
			//CefWeb.GetMainFrame().ExecuteJavaScriptAsync("document.appendStyle()");
		}
		#region Events
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			settings.WindowState = (int)WindowState;
			settings.WindowHeight = this.Height;
			settings.WindowWidth = this.Width;
			settings.Save();
		}
		#region MenuButtons


		private void Back_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			CefWeb.Back();
		}

		private void Forward_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			CefWeb.Forward();
		}

		private void Reload_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			CefWeb.Reload();
		}

		private void Home_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			CefWeb.Load(BaseURL);
		}

		private void Settings_Executed(object sender, ExecutedRoutedEventArgs e)
		{
			settingsMenu.ShowDialog();
		}

		private void Exit_Executed(object sender, ExecutedRoutedEventArgs e)
		{
		}
		#endregion
		#endregion
	}
	public class LSHandler : ILifeSpanHandler, IDownloadHandler
	{
		public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			Process.Start(downloadItem.OriginalUrl);
		}

		public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
		{ }

		bool ILifeSpanHandler.DoClose(IWebBrowser chromiumWebBrowser, IBrowser browser)
		{
			return true;
		}

		void ILifeSpanHandler.OnAfterCreated(IWebBrowser chromiumWebBrowser, IBrowser browser) { }

		void ILifeSpanHandler.OnBeforeClose(IWebBrowser chromiumWebBrowser, IBrowser browser) { }

		bool ILifeSpanHandler.OnBeforePopup(IWebBrowser chromiumWebBrowser, IBrowser browser, IFrame frame, string targetUrl, string targetFrameName, WindowOpenDisposition targetDisposition, bool userGesture, IPopupFeatures popupFeatures, IWindowInfo windowInfo, IBrowserSettings browserSettings, ref bool noJavascriptAccess, out IWebBrowser newBrowser)
		{
			Process.Start(targetUrl);
			newBrowser = null;
			return true;
		}
	}
}
