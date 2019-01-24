using System;
using System.Windows.Threading;

using RecognitionService.Models;

namespace RecognitionService.Input.Touch
{
	public class DeviceMock : IDeviceController, IInputProvider, IDisposable
	{
		private DispatcherTimer _statusTimer;

		private TouchPointFrameGenerator _touchPointFrameGenerator;

		public float ScreenWidth { get; private set; }
		public float ScreenHeight { get; private set; }

		public string DeviceName { get; } = "Device Mock";
		public DeviceState State { get; private set; } = DeviceState.Uninitialized;
		public event Action<DeviceState> OnStateChanged;

		public event Action<TouchPointFrame> OnTouchesRecieved;

		public DeviceMock() { }

		public void Init()
		{
			if (State == DeviceState.Uninitialized)
			{
				State = DeviceState.Initialized;
				OnStateChanged?.Invoke(State);
			}
		}

		public void Start()
		{
			if (State == DeviceState.Initialized)
			{
				State = DeviceState.Starting;
				OnStateChanged?.Invoke(State);

				_statusTimer = new DispatcherTimer(
					TimeSpan.FromSeconds(3),
					DispatcherPriority.Normal,
					(sender, eventArgs) =>
					{
						KillTimer();
						State = DeviceState.Running;
						OnStateChanged?.Invoke(State);
						_touchPointFrameGenerator = new TouchPointFrameGenerator();
						_touchPointFrameGenerator.OnTouchesRecieved += OnTouchesRecieved;
					},
					Dispatcher.CurrentDispatcher
				);
			}
		}

		public void Stop()
		{
			if (State == DeviceState.Running)
			{
				State = DeviceState.Initialized;
				OnStateChanged?.Invoke(State);
			}
		}

		public void Terminate()
		{
			KillTimer();
			Stop();
			State = DeviceState.Uninitialized;
			OnStateChanged?.Invoke(State);
		}

		private void KillTimer()
		{
			if (_statusTimer != null)
			{
				_statusTimer.Stop();
				_statusTimer = null;
			}
		}

		public void Dispose()
		{
			Terminate();
			
			if (_touchPointFrameGenerator != null) 
			{
				_touchPointFrameGenerator.OnTouchesRecieved -= OnTouchesRecieved;
			}
		}
	}
}
