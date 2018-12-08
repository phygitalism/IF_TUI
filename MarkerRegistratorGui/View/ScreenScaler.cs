using System.Numerics;
using System.Windows;

namespace MarkerRegistratorGui.View
{
	public class ScreenScaler
	{
		private readonly Window _window;

		public ScreenScaler(Window window)
		{
			_window = window;
		}

		public (int x, int y) ScaleAndSafePosition(Vector2 position)
		{
			var unscaledX = position.X;
			var unscaledY = position.Y;

			var scaledX = unscaledX * _window.Width;
			var scaledY = unscaledY * _window.Height;

			var screenPoint = _window.PointToScreen(new Point(scaledX, scaledY));

			return (
				SizeSafe((int)screenPoint.X, unscaledX),
				SizeSafe((int)screenPoint.Y, unscaledY)
			);
		}

		private int SizeSafe(int position, float unscaled)
			=> unscaled == 1.0f ? position - 1 : position;
	}
}
