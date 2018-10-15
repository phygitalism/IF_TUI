using System;

namespace RecognitionService
{
    public class DeviceMock : IDeviceController, IDisposable
    {
        private System.Windows.Threading.DispatcherTimer _statusTimer;

        public string DeviceName { get; } = "Device Mock";
        public DeviceState State { get; private set; } = DeviceState.Uninitialized;
        public event Action<DeviceState> OnStateChanged;

        public DeviceMock() { }

        private void KillTimer()
        {
            if (_statusTimer != null)
            {
                _statusTimer.Stop();
                _statusTimer = null;
            }
        }

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
                // Simulate a real device with a simple timer
                _statusTimer = new System.Windows.Threading.DispatcherTimer(
                    new TimeSpan(0, 0, 3),
                    System.Windows.Threading.DispatcherPriority.Normal,
                    (sender, eventArgs) =>
                    {
                        KillTimer();
                        State = DeviceState.Running;
                        _statusTimer = null;
                        OnStateChanged?.Invoke(State);
                    },
                    System.Windows.Threading.Dispatcher.CurrentDispatcher
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

        public void Dispose()
        {
            Terminate();
        }
    }
}
