using MaoMao.ViewModels;

namespace MaoMao.Views;

public partial class Settings : ContentPage
{
	public Settings(SettingsViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}
}