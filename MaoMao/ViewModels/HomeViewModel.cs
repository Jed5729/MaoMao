using MaoMao.Services;
using MaoMao.Views;

namespace MaoMao.ViewModels;

public partial class HomeViewModel : ObservableObject
{
	private AuthManager _auth;

	public HomeViewModel(AuthManager auth)
	{
		_auth = auth;
	}

	[RelayCommand]
	async Task GoToPlayer()
	{
		string url = "https://file-examples.com/wp-content/storage/2017/04/file_example_MP4_480_1_5MG.mp4";
		await Shell.Current.GoToAsync($"{nameof(Player)}?url={url}");
	}

	[RelayCommand]
	async Task SignIn() => await _auth.LoginUserAsync("jed", "test");

	[RelayCommand]
	async Task Register() => await _auth.RegisterUserAsync($"jed_{Random.Shared.Next(1000, 10000)}", "test", $"jacobysmonkey+MaoMao-{Guid.NewGuid()}");

	[RelayCommand]
	async Task LogOut() => await _auth.LogoutUserAsync();

	[RelayCommand]
	async Task SendVerification() => await _auth.SendVerificationAsync();
}
