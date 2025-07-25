using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Google.Android.Material.BottomNavigation;
using Microsoft.Maui.Controls.Handlers.Compatibility;
using Microsoft.Maui.Controls.Platform;
using Microsoft.Maui.Controls.Platform.Compatibility;

namespace MaoMao.Platforms.AndroidCustoms
{
	partial class ShellHandlerEx : ShellRenderer
	{
		protected override IShellBottomNavViewAppearanceTracker CreateBottomNavViewAppearanceTracker(ShellItem shellItem)
		{
			return new ShellBottomNavViewAppearanceTrackerEx(this, shellItem.CurrentItem);
		}
	}

	class ShellBottomNavViewAppearanceTrackerEx : ShellBottomNavViewAppearanceTracker
	{
		private readonly IShellContext shellContext;

		public ShellBottomNavViewAppearanceTrackerEx(IShellContext shellContext, ShellItem shellItem) : base(shellContext, shellItem)
		{
			this.shellContext = shellContext;
		}

		public override void SetAppearance(BottomNavigationView bottomView, IShellAppearanceElement appearance)
		{
			base.SetAppearance(bottomView, appearance);
			var backgroundDrawable = new GradientDrawable();
			backgroundDrawable.SetShape(ShapeType.Rectangle);
			//backgroundDrawable.SetCornerRadii(new float[] { 40, 40, 40, 40, 0, 0, 0, 0 });
			backgroundDrawable.SetTint(Application.Current?.RequestedTheme == AppTheme.Dark ? Android.Graphics.Color.Black : Android.Graphics.Color.LightGray);
			bottomView.SetBackground(backgroundDrawable);
			bottomView.SetPadding(0, 0, 0, 30);
		}
	}
}
