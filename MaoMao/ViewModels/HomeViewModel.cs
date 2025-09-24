using MaoMao.Views;

namespace MaoMao.ViewModels;

public partial class HomeViewModel : ObservableObject
{

	[RelayCommand]
	async Task GoToPlayer()
	{
		string url = "https://file-examples.com/wp-content/storage/2017/04/file_example_MP4_480_1_5MG.mp4";
		await Shell.Current.GoToAsync($"{nameof(Player)}?url={url}");
	}
}
