using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using MarkerRegistratorGui.Model;
using MarkerRegistratorGui.View.TouchInjection;
using MarkerRegistratorGui.ViewModel;

namespace MarkerRegistratorGui.View
{
	public class PointerInjector : IDisposable
	{
		private const int _maxPointers = 255;

		private static readonly TimeSpan _injectionDelay = TimeSpan.FromMilliseconds(125);

		static PointerInjector()
		{
			if (!TouchInjector.InitializeTouchInjection(_maxPointers, TouchFeedback.INDIRECT))
				throw new Exception("Couldn't initialize touch injection");
		}

		public static void InjectPointers(PointerTouchInfo[] touchInfo, int count)
		{
			if (count <= 0)
				return;

			LogInjection(touchInfo, count);

			if (!TouchInjector.InjectTouchInput(count, touchInfo))
				Debug.WriteLine($"Error while injecting: {new Win32Exception().Message}");
		}

		[Conditional("DEBUG")]
		private static void LogInjection(PointerTouchInfo[] touchInfo, int count)
		{
			Debug.WriteLine("Injection start");
			for (int i = 0; i < count; i++)
			{
				var touch = touchInfo[i];
				Debug.WriteLine($"Injecting {touch.PointerInfo.PointerId} {touch.PointerInfo.PointerFlags} {touch.PointerInfo.PtPixelLocation.X}:{touch.PointerInfo.PtPixelLocation.Y}");
			}
			Debug.WriteLine("Injection end");
		}

		private static PointerTouchInfo CreatePointerTouchInfo(int id, (int x, int y) position, TrackerEventType eventType)
			=> new PointerTouchInfo()
			{
				PointerInfo =
				{
					PointerId = (uint)id,
					PointerType = PointerInputType.TOUCH,
					PointerFlags = GetFlagsForEvent(eventType),
					PtPixelLocation =
					{
						X = position.x,
						Y = position.y
					}
				}
			};

		private static PointerFlags GetFlagsForEvent(TrackerEventType eventType)
		{
			switch (eventType)
			{
				case TrackerEventType.Down:
					return PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.DOWN;
				case TrackerEventType.Update:
					return PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.UPDATE;
				case TrackerEventType.Up:
					return PointerFlags.UP;
				default:
					throw new NotSupportedException();
			}
		}

		private readonly ScreenScaler _screenScaler;
		private readonly PointersViewModel _pointersViewModel;

		private readonly PointerTouchInfo[] _pointersBuffer = new PointerTouchInfo[_maxPointers];
		private readonly Dictionary<int, (int x, int y)> _injectedPointers
			= new Dictionary<int, (int x, int y)>();

		private CancellationTokenSource _autoUpdateCancellation;

		public PointerInjector(ScreenScaler screenScaler, PointersViewModel pointersViewModel)
		{
			_screenScaler = screenScaler;
			_pointersViewModel = pointersViewModel;

			_pointersViewModel.WhenPointerEvent.Subscribe(HandlePointerUpdates);
		}

		public void HandlePointerUpdates(IEnumerable<TrackerEvent<PointerState>> value)
		{
			Debug.WriteLine("Event update");

			var updatedIds = new List<int>();

			CancelAutoUpdate();
			var i = 0;
			foreach (var e in value)
			{
				Debug.WriteLine($"Pointer id {e.id} event {e.type} pos {e.state.position}");

				var position = _screenScaler.ScaleAndSafePosition(e.state.position);

				_pointersBuffer[i] = CreatePointerTouchInfo(e.id, position, e.type);

				if (e.type == TrackerEventType.Up)
					_injectedPointers.Remove(e.id);
				else
					_injectedPointers[e.id] = position;

				updatedIds.Add(e.id);
				i++;
			}

			foreach (var pair in _injectedPointers)
			{
				var id = pair.Key;
				var position = pair.Value;

				if (updatedIds.Contains(id))
					continue;

				_pointersBuffer[i] = CreatePointerTouchInfo(id, position, TrackerEventType.Update);
				i++;
			}

			InjectPointers(_pointersBuffer, i);
			SheduleAutoUpdate();
		}

		private void CancelAutoUpdate()
			=> _autoUpdateCancellation?.Cancel();

		private async void SheduleAutoUpdate()
		{
			try
			{
				_autoUpdateCancellation = new CancellationTokenSource();

				while (true)
				{
					await Task.Delay(_injectionDelay, _autoUpdateCancellation.Token);
					AutoUpdate();
				}

			}
			catch (OperationCanceledException)
			{
				Debug.WriteLine("AutoUpdate canceled");
			}
			catch (ObjectDisposedException)
			{

			}
		}

		private void AutoUpdate()
		{
			Debug.WriteLine("AutoUpdate");

			var i = 0;
			foreach (var pair in _injectedPointers)
			{
				_pointersBuffer[i] = CreatePointerTouchInfo(pair.Key, pair.Value, TrackerEventType.Update);
				i++;
			}

			InjectPointers(_pointersBuffer, i);
		}

		public void Dispose()
		{
			_autoUpdateCancellation?.Dispose();
		}
	}
}
