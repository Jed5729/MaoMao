using MaoMao.Services;
using Themes = MaoMao.Resources.Themes;

namespace MaoMao.ViewModels;

public partial class SettingsViewModel : ObservableObject
{
	private ThemeManager _themes;
	public SettingsViewModel(ThemeManager themeManager)
	{
		_themes = themeManager;
	}

	[RelayCommand]
	public void DarkTheme()
	{
		_themes.SetTheme(nameof(Themes.Dark));
	}

	[RelayCommand]
	public void LightTheme()
	{
		_themes.SetTheme(nameof(Themes.Light));
	}

	[RelayCommand]
	public void MaoMaoTheme()
	{
		_themes.SetTheme(nameof(Themes.MaoMao));
	}
}
