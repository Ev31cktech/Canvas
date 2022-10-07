using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace Canvas
{
	public partial class SettingsMenu : Window
	{
		private static TextSettingItem userName = new TextSettingItem("UserName");
		private static PasswordSettingItem password = new PasswordSettingItem("Password");
		private static BoolSettingItem darkMode = new BoolSettingItem("Darkmode");
		private static BoolSettingItem keepmenuBar = new BoolSettingItem("KeepmenuBar");
		public String UserName{ get { return userName.Value; } }
		public String Password{ get { return password.Value; } }
		public Boolean DarkMode { get { return darkMode.Value; } }
		public Boolean KeepMenuBar { get { return keepmenuBar.Value; } }

		private List<SettingItemBase> settingsList = new List<SettingItemBase>()
		{
			userName,
			password,
			darkMode,
			keepmenuBar
		};

		public SettingsMenu()
		{
			InitializeComponent();
			foreach (SettingItemBase si in settingsList)
			{
				SettingGrid.RowDefinitions.Add(new RowDefinition());
				int rc = SettingGrid.RowDefinitions.Count;
				Grid.SetRow(si.label, rc - 1);
				Grid.SetRow(si.control, rc - 1);
				Grid.SetColumn(si.label, 0);
				Grid.SetColumn(si.control, 1);
			}
		}
		public abstract class SettingItemBase
		{
			public Label label { get; internal set; }
			public Control control { get; internal set; }
		}

		public abstract class SettingItem<T> : SettingItemBase
			{
			public T Value { get; internal set; }
			public SettingItem(string lblText)
			{
				label = new Label() { Content = lblText };
			}
		}
		public class TextSettingItem : SettingItem<String>
		{
			public TextSettingItem(string lblText) : base(lblText)
			{
				control = new TextBox();
			}

		}
		public class PasswordSettingItem :  SettingItem<String>
		{
			public PasswordSettingItem(string lblText) : base(lblText)
			{
				control = new PasswordBox() { };
			}

		}
		public class BoolSettingItem : SettingItem<bool>
		{

			public BoolSettingItem(string lblText) : base(lblText)
			{
				control = new CheckBox() { };
			}
		}

		internal String Save()
		{
			return "";
		}
	}
}
