using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RecognitionService
{
    public interface IDeviceController
    {
        string DeviceName { get; }
        DeviceState State { get; }
        
        void Init();
        void Start();
        void Stop();
    }
}
