using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using MarkerRegistratorGui.Model;
using MarkerRegistratorGui.View.TouchInjection;
using MarkerRegistratorGui.ViewModel;

namespace MarkerRegistratorGui.View
{
	public class PointerInjector : IDisposable
	{
		private static readonly TimeSpan _injectionDelay = TimeSpan.FromMilliseconds(125);

		static PointerInjector()
		{
			if (!TouchInjector.InitializeTouchInjection(255, TouchFeedback.INDIRECT))
				throw new Exception("Couldn't initialize touch injection");
		}

		public static void InjectPointers(IEnumerable<InjectedPointerState> updates)
		{
			var touchInfo = updates
				.Select(CreatePointerTouchInfo)
				.ToArray();

			foreach (var touch in touchInfo)
				Debug.WriteLine($"Injecting {touch.PointerInfo.PointerId} {touch.PointerInfo.PointerFlags}");

			if (touchInfo.Length != 0 && !TouchInjector.InjectTouchInput(touchInfo.Length, touchInfo))
				throw new Win32Exception();
		}

		private static PointerTouchInfo CreatePointerTouchInfo(InjectedPointerState state)
			=> new PointerTouchInfo()
			{
				PointerInfo =
					{
						PointerId = (uint)state.Id,
						PointerType = PointerInputType.TOUCH,
						PointerFlags = GetFlags(state),
						PtPixelLocation =
						{
							X = state.PosX,
							Y = state.PosY
						}
					}
			};

		private static PointerFlags GetFlags(InjectedPointerState state)
		{
			var result = PointerFlags.NONE;

			if (state.IsAlive)
			{
				result |= PointerFlags.INRANGE | PointerFlags.INCONTACT;

				if (state.IsNew)
					result |= PointerFlags.DOWN;
				else
					result |= PointerFlags.UPDATE;
			}
			else
				result |= PointerFlags.UPDATE | PointerFlags.UP;

			return result;
		}

		private readonly Window _window;
		private readonly PointersViewModel _pointersViewModel;

		private readonly IDisposable _subscription;

		private readonly Dictionary<int, InjectedPointerState> _pointerStates
			= new Dictionary<int, InjectedPointerState>();

		private readonly Task _injectionTask;
		private readonly CancellationTokenSource _injectionCancellation
			= new CancellationTokenSource();

		public PointerInjector(Window window, PointersViewModel pointersViewModel)
		{
			_window = window;
			_pointersViewModel = pointersViewModel;

			_subscription = _pointersViewModel.WhenPointerEvent.Subscribe(HandlePointerUpdates);

			_injectionTask = Task.Run(() => InjectionCycle(_injectionCancellation.Token));
		}

		private async Task InjectionCycle(CancellationToken cancellationToken)
		{
			try
			{
				while (!cancellationToken.IsCancellationRequested)
				{
					lock(_pointerStates)
					if (_pointerStates.Count > 0)
					{
						InjectPointers(_pointerStates.Values);
						UpdateState();
					}

					await Task.Delay(_injectionDelay);
				}
			}
			catch (Exception e)
			{
				Debug.WriteLine($"Error in injection cycle: {e.Message}");
			}
		}

		private void UpdateState()
		{
			foreach (var state in _pointerStates.Values)
			{
				if (!state.IsAlive)
					_pointerStates.Remove(state.Id);
				else if (state.IsNew)
					state.IsNew = false;
			}
		}

		public void HandlePointerUpdates(IEnumerable<TrackerEvent<PointerState>> value)
		{
			lock(_pointerStates)
			foreach (var e in value)
			{
				Debug.WriteLine($"Pointer id {e.id} event {e.type} pos {e.state.position}");

				if (!_pointerStates.TryGetValue(e.id, out var state))
						_pointerStates.Add(
						e.id,
						state = new InjectedPointerState()
						{
							Id = e.id,
							IsNew = true
						}
					);

				if (e.type == TrackerEventType.Down)
					state.IsAlive = true;
				else if (e.type == TrackerEventType.Up)
					state.IsAlive = false;

				(state.PosX, state.PosY) = ScaleAndSafePosition(e.state.position);
			}
		}

		private (int x, int y) ScaleAndSafePosition(Vector2 position)
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

		public void Dispose()
		{
			_subscription.Dispose();

			_injectionCancellation.Cancel();
			_injectionTask.Wait();
		}

		public class InjectedPointerState
		{
			public int Id { get; set; }
			public int PosX { get; set; }
			public int PosY { get; set; }
			public bool IsNew { get; set; }
			public bool IsAlive { get; set; }
		}
	}
}
