using Themes = MaoMao.Resources.Themes;

namespace MaoMao.Services;

public class ThemeManager
{
	private const string ThemeKey = "theme";

	private readonly IDictionary<string, ResourceDictionary> _themesMap = new Dictionary<string, ResourceDictionary>()
	{
		[nameof(Themes.MaoMao)] = new Themes.MaoMao(),
		[nameof(Themes.Dark)] = new Themes.Dark(),
		[nameof(Themes.Light)] = new Themes.Light()
	};

	public string SelectedTheme { get; set; } = "";

	public void SetTheme(string themeName)
	{
		if(SelectedTheme == themeName) return;

		var themeToBeApplied = _themesMap[themeName];

		if (Application.Current is null) return;
		var existingTheme = Application.Current.Resources.MergedDictionaries.FirstOrDefault(d => _themesMap.Values.Contains(d));
		if (existingTheme != null)
			Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
		Application.Current.Resources.MergedDictionaries.Add(themeToBeApplied);
		SelectedTheme = themeName;

		Preferences.Default.Set(ThemeKey, themeName);
	}

	public string GetSavedThemeOrDefault()
	{
		var theme = Preferences.Default.Get(ThemeKey, nameof(Themes.MaoMao));
		return theme;
	}
}
