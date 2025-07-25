using MaoMao.Models;
using MaoMao.ViewModels;
using MauiIcons.Core;
using MauiIcons.Cupertino;

namespace MaoMao.Views;

public partial class AndroidMore : ContentPage
{
	public AndroidMore(AndroidMoreViewModel vm)
	{
		InitializeComponent();
		_ = new MauiIcon();
		vm.Pages = new List<MorePage>()
		{
			new("History", CupertinoIcons.ArrowClockwise, nameof(History)),
			new("Account", CupertinoIcons.PersonCropCircleFill, nameof(Account)),
			new("Settings", CupertinoIcons.Gear, nameof(Settings))
		};
		BindingContext = vm;
	}
}