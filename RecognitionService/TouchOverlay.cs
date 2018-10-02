using System;

namespace RecognitionService
{
    class TouchOverlay : IDeviceController
    {
        public string DeviceName { get; private set; }
        public DeviceState State { get; private set; }
        public delegate void StateChangedEvent();
        public event StateChangedEvent OnStateChanged;

        public TouchOverlay()
        {
            DeviceName = "PQ LABS Touch Overlay";
            State = DeviceState.Uninitialized;
        }

        public void Init()
        {
            if (State == DeviceState.Uninitialized)
            {
                BindToTouchOverlayEvents();
                State = DeviceState.Initialized;
            }
            
        }

        public void Start()
        {
            if (State == DeviceState.Initialized)
            {
                State = DeviceState.Starting;
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
            Stop();
            State = DeviceState.Uninitialized;
        }

        private void BindToTouchOverlayEvents() {
            
        }
    }
}
