using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Timers;
using System.Windows;
using System.Windows.Input;
using System.Windows.Shell;
using CefSharp;
using CefSharp.Web;
using CefSharp.Wpf;
using Newtonsoft.Json.Linq;

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
	///	
	/// [] better file viewer
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
		const String BaseURL = "https://fhict.instructure.com";
		const String FILE_IFRAME_SOURCE_JS_REQUEST = "document.getElementById('doc_preview').children[0].children[0].src";
		public MainWindow()
		{
			WindowState = (WindowState)settings.WindowState;
			Height = settings.WindowHeight;
			Width = settings.WindowWidth;
			string cache_dir = Environment.CurrentDirectory + @"\cache";
			Cef.Initialize(new CefSettings() { CachePath = cache_dir });
			InitializeComponent();
			TopMenu_SetVisible(settingsMenu.KeepMenuBar);
			KeepMenuBarCBX.IsChecked = settingsMenu.KeepMenuBar;
			CefWeb.LifeSpanHandler = new LSHandler();
			CefWeb.DownloadHandler = new DLHandler(this);
			CefWeb.FrameLoadStart += CefWeb_FrameLoadStart;
			CefWeb.FrameLoadEnd += CefWeb_FrameLoadEnd;
			CefWeb.Address = BaseURL;
			CefWeb.AddressChanged += CefWeb_AddressChanged;
			CefWeb.Focus();

			InputManager.Current.EnterMenuMode += this.InputManager_MenuModeToggled;
			InputManager.Current.LeaveMenuMode += this.InputManager_MenuModeToggled;

			//CefWeb.GetMainFrame().ExecuteJavaScriptAsync("document.appendStyle()");
		}

		private void CefWeb_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
		{
		}

		private void CefWeb_FrameLoadStart(object sender, FrameLoadStartEventArgs e)
		{
		}

		private async void CefWeb_AddressChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			String newUrl = e.NewValue as String;
			if (e.NewValue != null)
			{
				if (newUrl.Contains("/files/"))
				{
					try
					{
						String fileID = newUrl.Split('/').Last().Split('?')[0];
						await CefWeb.LoadUrlAsync($"{BaseURL}/api/v1/files/{fileID}");
						await CefWeb.GetMainFrame().GetSourceAsync().ContinueWith((Task<String> result) =>
						{
							JValue jValue = JObject.Parse(Regex.Match(result.Result, "{.*}").Value).SelectToken("canvadoc_session_url") as JValue;
							CefWeb.LoadUrl(BaseURL + jValue.Value as string);
						});
					}
					catch (Exception err)
					{}
				}
			}
		}
		#region Events
		private void InputManager_MenuModeToggled(object sender, EventArgs e)
		{
			TopMenu_SetVisible(InputManager.Current.IsInMenuMode || KeepMenuBarCBX.IsChecked);
		}
		private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Cef.Shutdown();
			settings.WindowState = (int)WindowState;
			settings.WindowHeight = this.Height;
			settings.WindowWidth = this.Width;
			settings.AppSettings = settingsMenu.Save();
			settings.Save();
			Application.Current.Shutdown();
		}
		#region MenuButtons
		private void Back_Executed(object sender, RoutedEventArgs e)
		{
			CefWeb.Back();
		}

		private void Forward_Executed(object sender, RoutedEventArgs e)
		{
			CefWeb.Forward();
		}

		private void Reload_Executed(object sender, RoutedEventArgs e)
		{
			CefWeb.Reload();
		}

		private void Home_Executed(object sender, RoutedEventArgs e)
		{
			CefWeb.Load(BaseURL);
		}

		private void Settings_Executed(object sender, RoutedEventArgs e)
		{
			settingsMenu.ShowDialog();
		}

		private void Exit_Executed(object sender, RoutedEventArgs e)
		{
		}
		#endregion


		#endregion
		internal void TaskBar_Update(TaskbarItemProgressState progressState, double progressValue)
		{
			if (this.TaskbarItemInfo == null)
				this.TaskbarItemInfo = new TaskbarItemInfo();
			this.TaskbarItemInfo.ProgressState = progressState;
			this.TaskbarItemInfo.ProgressValue = progressValue;
		}

		private void KeepMenuBarCBX_Click(object sender, RoutedEventArgs e)
		{
			TopMenu_SetVisible(KeepMenuBarCBX.IsChecked);
		}

		private void TopMenu_SetVisible(bool visibility)
		{
			if (visibility)
				TopMenu.Height = double.NaN;
			else
				TopMenu.Height = 0;
		}
		public static string PropertyList(object obj)
		{
			var props = obj.GetType().GetProperties();
			var sb = new StringBuilder();
			foreach (var p in props)
			{
				sb.AppendLine(p.Name + ": " + p.GetValue(obj, null));
			}
			return sb.ToString();
		}

		private void OpenFile_Click(object sender, RoutedEventArgs e)
		{
			CefWeb.LoadUrl("https://fhict.instructure.com/courses/12697/files/1726280?module_item_id=848988");
		}
	}
	public class DLHandler : IDownloadHandler
	{
		int TaskBarAnimPerc;
		Timer TaskBarAnimTimer;
		MainWindow thisWindow;
		public DLHandler(MainWindow mw)
		{
			TaskBarAnimPerc = 0;
			thisWindow = mw;
			TaskBarAnimTimer = new Timer(1);
			TaskBarAnimTimer.Elapsed += TaskBarAnimTimer_Elapsed;
		}
		private void TaskBarAnimTimer_Elapsed(object sender, ElapsedEventArgs e)
		{
			thisWindow.Dispatcher.Invoke(() => { thisWindow.TaskBar_Update(TaskbarItemProgressState.Normal, (100 - TaskBarAnimPerc) / 100); });
			if (TaskBarAnimPerc >= 100)
			{
				thisWindow.Dispatcher.Invoke(() => { thisWindow.TaskBar_Update(TaskbarItemProgressState.None, 0); });
				TaskBarAnimTimer.Stop();
			}
			TaskBarAnimPerc++;
		}

		public bool CanDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, string url, string requestMethod)
		{
			return true;
		}

		public void OnBeforeDownload(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IBeforeDownloadCallback callback)
		{
			string fullpath = Environment.ExpandEnvironmentVariables("%userprofile%\\downloads\\" + downloadItem.SuggestedFileName);
			int FileNum = 0;
			while (File.Exists(fullpath))
			{
				FileNum++;
				String[] path = fullpath.Split('.');
				if (FileNum == 1)
					path[0] += " ";
				path[0] = path[0].Split('(')[0];
				fullpath = String.Join($"({FileNum}).", path);
			}
			callback.Continue(fullpath, false);
		}

		public void OnDownloadUpdated(IWebBrowser chromiumWebBrowser, IBrowser browser, DownloadItem downloadItem, IDownloadItemCallback callback)
		{
			thisWindow.Dispatcher.Invoke(() => thisWindow.TaskBar_Update(TaskbarItemProgressState.Normal, downloadItem.PercentComplete / 100));
			if (downloadItem.IsComplete)
			{
				TaskBarAnimTimer.Start();
			}
			Console.WriteLine(MainWindow.PropertyList(downloadItem));
		}
	}
	public class LSHandler : ILifeSpanHandler
	{
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
