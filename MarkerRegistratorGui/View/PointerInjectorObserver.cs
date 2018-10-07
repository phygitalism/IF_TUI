using System;
using System.Windows;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.View
{
	public class PointerInjectorObserver : IObserver<TrackerEvent<PointerState>>
	{
		private readonly Window _window;

		public PointerInjectorObserver(Window window)
		{
			_window = window;
		}

		public void OnCompleted() => throw new NotImplementedException();
		public void OnError(Exception error) => throw new NotImplementedException();

		public void OnNext(TrackerEvent<PointerState> e)
		{
			var unscaledX = e.state.position.X;
			var unscaledY = e.state.position.Y;

			var scaledX = unscaledX * _window.Width;
			var scaledY = unscaledY * _window.Height;

			var screenPoint = _window.PointToScreen(new Point(scaledX, scaledY));

			GetInjectAction(e.type).Invoke(
				e.id,
				SizeSafe((int)screenPoint.X, unscaledX),
				SizeSafe((int)screenPoint.Y, unscaledY)
			);
		}

		private int SizeSafe(int position, float unscaled)
			=> unscaled == 1.0f ? position - 1 : position;

		private Action<int, int, int> GetInjectAction(TrackerEventType type)
		{
			switch (type)
			{
				case TrackerEventType.Up:
					return PointerInjection.PointerInjector.InjectPointerUp;
				case TrackerEventType.Down:
					return PointerInjection.PointerInjector.InjectPointerDown;
				case TrackerEventType.Update:
					return PointerInjection.PointerInjector.InjectPointerUpdate;
				default:
					throw new NotSupportedException();
			}
		}
	}
}
