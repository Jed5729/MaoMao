using MaoMao.ViewModels;
using MauiIcons.Core;

namespace MaoMao.Views;

public partial class Home : ContentPage
{
    public Home(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
