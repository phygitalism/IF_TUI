using System.Diagnostics;
using System.Numerics;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace MarkerRegistratorGui.View
{
	public class ScreenScaler
	{
		private readonly Window _window;

		private (double X, double Y) _scale;
		private (double Width, double Height) _size;
		private (double X, double Y) _position;

		public ScreenScaler(Window window)
		{
			_window = window;

			_window.Loaded += (sender, args) =>
			{
				UpdateScale((Window)sender);
				UpdateSizeAndPos((Window)sender);
			};
			_window.SizeChanged += (sender, args) => UpdateSizeAndPos((Window)sender);
			_window.LocationChanged += (sender, args) => UpdateSizeAndPos((Window)sender);
		}

		private void UpdateSizeAndPos(Window window)
		{
			_position = (window.Left, window.Top);
			_size = (window.Width, window.Height);

			Debug.WriteLine($"Position = {_position}");
			Debug.WriteLine($"Size = {_size}");
		}

		private void UpdateScale(Window window)
		{
			Matrix matrix;
			var source = PresentationSource.FromVisual(window);
			if (source != null)
			{
				matrix = source.CompositionTarget.TransformToDevice;
			}
			else
			{
				using (var src = new HwndSource(new HwndSourceParameters()))
				{
					matrix = src.CompositionTarget.TransformToDevice;
				}
			}

			_scale = (matrix.M11, matrix.M22);

			Debug.WriteLine($"Scale = {_scale}");
		}

		public (int x, int y) ScaleAndSafePosition(Vector2 position)
		{
			var unscaledX = position.X;
			var unscaledY = position.Y;

			var scaledX = unscaledX * _size.Width * _scale.X;
			var scaledY = unscaledY * _size.Height * _scale.Y;

			var absoluteX = _position.X + scaledX;
			var absoluteY = _position.Y + scaledY;

			return (
				SizeSafe((int)absoluteX, unscaledX),
				SizeSafe((int)absoluteY, unscaledY)
			);
		}

		private int SizeSafe(int position, float unscaled)
			=> unscaled == 1.0f ? position - 1 : position;
	}
}
