namespace MaoMao.Services;

public interface IThemeManager
{
	string SelectedTheme { get; set; }

	string GetSavedThemeOrDefault();
	void SetTheme(string themeName);
}