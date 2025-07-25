using MaoMao.Models;
using MaoMao.Views;

namespace MaoMao.ViewModels;

public partial class AndroidMoreViewModel : ObservableObject
{
	private bool _isNavigating = false;

	[ObservableProperty]
	private List<MorePage>? pages;

	[RelayCommand]
	async Task Navigate(string route)
	{
		if (_isNavigating || string.IsNullOrWhiteSpace(route))
			return;

		_isNavigating = true;

		try
		{
			await Shell.Current.GoToAsync(route);
		}
		finally
		{
			// Small delay to prevent multiple navigations in rapid taps
			await Task.Delay(500);
			_isNavigating = false;
		}
	}
}
