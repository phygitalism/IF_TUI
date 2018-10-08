using System;

namespace RecognitionService
{
    public class DeviceMock : IDeviceController
    {
        private System.Windows.Threading.DispatcherTimer _statusTimer;

        public string DeviceName { get; private set; }
        public DeviceState State { get; private set; } = DeviceState.Uninitialized;
        public delegate void StateChangedEvent();
        public event StateChangedEvent OnStateChanged;

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
            }

            DeviceName = "PQ LABS Touch Overlay";
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
                        OnStateChanged?.Invoke();
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
                OnStateChanged?.Invoke();
            }
        }

        public void Terminate()
        {
            KillTimer();
            Stop();
            State = DeviceState.Uninitialized;
        }
    }
}
