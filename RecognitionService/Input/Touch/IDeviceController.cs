using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognitionService.Input.Touch
{
    public interface IDeviceController
    {
        string DeviceName { get; }
        DeviceState State { get; }
        event Action<DeviceState> OnStateChanged;
        
        void Init();
        void Start();
        void Stop();
        void Terminate();
    }
}
