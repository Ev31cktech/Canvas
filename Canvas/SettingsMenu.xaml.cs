using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;

namespace Canvas
{
	public partial class SettingsMenu : Window
	{
		private List<SettingItem> settingsList = new List<SettingItem>()
		{
			new TextSettingItem("UserName"),
			new PasswordSettingItem("Password")
		};
		public SettingsMenu()
		{
			InitializeComponent();
			foreach (SettingItem si in settingsList)
			{
				SettingGrid.RowDefinitions.Add(new RowDefinition());
				int rc = SettingGrid.RowDefinitions.Count;
				Grid.SetRow(si.label, rc - 1);
				Grid.SetRow(si.control, rc - 1);
				Grid.SetColumn(si.label, 0);
				Grid.SetColumn(si.control, 1);
			}
		}
	}
	internal class SettingItem
	{
		public Label label { get; internal set; }
		public Control control { get; internal set; }
	}
	internal class TextSettingItem : SettingItem
	{
		public TextSettingItem(string labelText)
		{
			label = new Label() { Content = labelText };
			control = new TextBox();
		}
	}
	internal class PasswordSettingItem : SettingItem
	{
		public PasswordSettingItem(string labelText)
		{
			label = new Label() { Content = labelText };
			control = new PasswordBox() { };

		}
	}
}
