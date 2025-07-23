namespace MaoMao.ViewModels;

[QueryProperty("Url", "url")]
public partial class PlayerViewModel : ObservableObject
{
	[ObservableProperty]
	string url;
}
