using System;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using MarkerRegistratorGui.View.TouchInjection;

namespace MarkerRegistratorGui.View
{
	public class PointerInjectionThread : IDisposable
	{
		private const float _injectionTick = 30;
		private const int _maxPointers = 255;

		static PointerInjectionThread()
		{
			if (!TouchInjector.InitializeTouchInjection(_maxPointers, TouchFeedback.INDIRECT))
				throw new Exception("Couldn't initialize touch injection");
		}

		private readonly Thread _injectionThread;

		private readonly Injection[] _injections = new Injection[_maxPointers];
		private readonly PointerTouchInfo[] _injectionPointers = new PointerTouchInfo[_maxPointers];

		private bool _isRunning;

		public PointerInjectionThread()
		{
			_injectionThread = new Thread(InjectionCycle);

			_isRunning = true;
			_injectionThread.Start();
		}

		private void InjectionCycle()
		{
			while (_isRunning)
			{
				try
				{
					InjectPointers();
				}
				catch (Exception e)
				{
					Debug.WriteLine(e.Message);
				}
				Thread.Sleep((int)(1000 / _injectionTick));
			}
		}

		private void InjectPointers()
		{
			var pointersCount = 0;
			for (int id = 0; id < _injections.Length; id++)
			{
				var injection = _injections[id];

				if (injection == null)
					continue;

				if (injection.NeedInjection || injection.IsInjected)
					_injectionPointers[pointersCount++] = CreatePointerForInjection(id, injection);

				if (injection.Dispose)
				{
					if (injection.IsInjected)
						injection.NeedExtraction = true;
					else
						_injections[id] = null;
				}
			}

			if (pointersCount == 0)
				return;

			Debug.WriteLine($"Injecting {pointersCount} pointers");
			TouchInjector.InjectTouchInput(pointersCount, _injectionPointers);
		}

		private static PointerTouchInfo CreatePointerForInjection(int id, Injection injection)
			=> new PointerTouchInfo
			{
				PointerInfo =
				{
					PointerId = (uint)id,
					PointerType = PointerInputType.TOUCH,
					PointerFlags = GetInjectionFlags(injection),
					PtPixelLocation =
					{
						X = injection.Position.x,
						Y = injection.Position.y
					}
				}
			};

		private static PointerFlags GetInjectionFlags(Injection injection)
		{
			if (!injection.IsInjected && injection.NeedInjection)
			{
				injection.NeedInjection = false;
				injection.IsInjected = true;

				return PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.DOWN;
			}

			if (injection.IsInjected && !injection.NeedInjection && !injection.NeedExtraction)
				return PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.UPDATE;

			if (injection.IsInjected && injection.NeedExtraction)
			{
				injection.IsInjected = false;
				injection.NeedExtraction = false;

				return PointerFlags.UP;
			}

			throw new InvalidOperationException();
		}

		public InjectionHandle CreateInjection()
		{
			for (int i = 0; i < _injections.Length; i++)
				if (_injections[i] == null)
					return new InjectionHandle(_injections[i] = new Injection());

			throw new InvalidOperationException("No free injections");
		}

		public void Dispose()
		{
			Debug.WriteLine("Waiting for injection thread to stop");

			_isRunning = false;
			_injectionThread.Join();
		}

		public class Injection
		{
			public (int x, int y) Position;

			public bool IsInjected;
			public bool NeedInjection;
			public bool NeedExtraction;
			public bool Dispose;
		}

		public struct InjectionHandle : IDisposable
		{
			private readonly Injection _injection;

			public InjectionHandle(Injection injection) : this()
			{
				_injection = injection;
			}

			public void Inject()
			{
				CheckDispose();
				_injection.NeedInjection = true;
			}

			public void Extract()
			{
				CheckDispose();
				_injection.NeedExtraction = true;
			}

			public void UpdatePosition(int x, int y)
			{
				CheckDispose();
				_injection.Position = (x, y);
			}

			public void Dispose()
			{
				CheckDispose();
				_injection.Dispose = true;
			}

			private void CheckDispose()
			{
				if (_injection.Dispose)
					throw new ObjectDisposedException(nameof(InjectionHandle));
			}
		}
	}
}
