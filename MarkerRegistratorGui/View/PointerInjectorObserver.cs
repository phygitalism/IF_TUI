using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Windows;
using MarkerRegistratorGui.Model;
using MarkerRegistratorGui.View.PointerInjection;

namespace MarkerRegistratorGui.View
{
	public class PointerInjectorObserver : IObserver<IEnumerable<TrackerEvent<PointerState>>>
	{
		private readonly Window _window;

		public PointerInjectorObserver(Window window)
		{
			_window = window;
		}

		public void OnCompleted() => throw new NotImplementedException();
		public void OnError(Exception error) => throw new NotImplementedException();

		public void OnNext(IEnumerable<TrackerEvent<PointerState>> value)
		{
			var updates = value.Select(e => new PointerInjector.PointerUpdate(
				e.id,
				ScaleAndSafePosition(e.state.position),
				e.type
			));

			PointerInjector.InjectPointers(updates);
		}

		private System.Drawing.Point ScaleAndSafePosition(Vector2 position)
		{
			var unscaledX = position.X;
			var unscaledY = position.Y;

			var scaledX = unscaledX * _window.Width;
			var scaledY = unscaledY * _window.Height;

			var screenPoint = _window.PointToScreen(new Point(scaledX, scaledY));

			return new System.Drawing.Point(
				SizeSafe((int)screenPoint.X, unscaledX),
				SizeSafe((int)screenPoint.Y, unscaledY)
			);
		}

		private int SizeSafe(int position, float unscaled)
			=> unscaled == 1.0f ? position - 1 : position;
	}
}
