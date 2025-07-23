using MaoMao.Views;

namespace MaoMao.ViewModels;

public partial class HomeViewModel : ObservableObject
{
	[RelayCommand]
	async Task GoToPlayer()
	{
		string url = "https://download.samplelib.com/mp4/sample-30s.mp4";
		await Shell.Current.GoToAsync($"{nameof(Player)}?url={url}");
	}
}
