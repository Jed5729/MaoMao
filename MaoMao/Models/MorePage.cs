using MauiIcons.Core;
using MauiIcons.Cupertino;

namespace MaoMao.Models;

public class MorePage(string title, CupertinoIcons icon, string route)
{
	public string Title { get; set; } = title;
	public CupertinoIcons PageIcon { get; set; } = icon;
	public string Route { get; set; } = route;
}
