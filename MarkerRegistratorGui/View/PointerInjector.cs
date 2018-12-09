using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive.Disposables;
using MarkerRegistratorGui.Model;
using MarkerRegistratorGui.ViewModel;

namespace MarkerRegistratorGui.View
{
	public class PointerInjector : IDisposable
	{
		private readonly ScreenScaler _screenScaler;
		private readonly PointersViewModel _pointersViewModel;
		private readonly PointerInjectionThread _pointerInjectionThread;
		private readonly IDisposable _disposable;

		private readonly Dictionary<int, PointerInjectionThread.InjectionHandle> _injections
			= new Dictionary<int, PointerInjectionThread.InjectionHandle>();

		public PointerInjector(ScreenScaler screenScaler, PointersViewModel pointersViewModel)
		{
			_screenScaler = screenScaler;
			_pointersViewModel = pointersViewModel;

			_pointerInjectionThread = new PointerInjectionThread();

			_disposable = new CompositeDisposable
			{
				_pointerInjectionThread,
				_pointersViewModel.WhenPointerEvent.Subscribe(HandlePointerUpdates)
			};
		}

		public void HandlePointerUpdates(IEnumerable<TrackerEvent<PointerState>> update)
		{
			Debug.WriteLine("Event update");

			foreach (var e in update)
			{
				(var x, var y) = _screenScaler.ScaleAndSafePosition(e.state.position);

				Debug.WriteLine($"Injecting id:{e.id} event:{e.type} pos:({x}:{y})");

				switch (e.type)
				{
					case TrackerEventType.Down:
						AddPointer(e.id, x, y);
						break;
					case TrackerEventType.Update:
						UpdatePointer(e.id, x, y);
						break;
					case TrackerEventType.Up:
						RemovePointer(e.id, x, y);
						break;
				}
			}
		}

		private void AddPointer(int id, int x, int y)
		{
			var injection = _pointerInjectionThread.CreateInjection();
			_injections.Add(id, injection);

			injection.UpdatePosition(x, y);
			injection.Inject();
		}

		private void UpdatePointer(int id, int x, int y)
		{
			var injection = _injections[id];

			injection.UpdatePosition(x, y);
		}

		private void RemovePointer(int id, int x, int y)
		{
			var injection = _injections[id];

			injection.UpdatePosition(x, y);
			injection.Extract();
			injection.Dispose();

			_injections.Remove(id);
		}

		public void Dispose()
			=> _disposable.Dispose();
	}
}
