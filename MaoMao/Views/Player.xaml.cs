using MaoMao.ViewModels;

namespace MaoMao.Views;

public partial class Player : ContentPage
{
	public Player(PlayerViewModel vm)
	{
		InitializeComponent();
		BindingContext = vm;
	}

	protected override void OnDisappearing()
	{
		base.OnDisappearing();
		player.Stop();
		player.Dispose();
	}
}