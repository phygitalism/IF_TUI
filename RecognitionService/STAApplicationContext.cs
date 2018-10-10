using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace RecognitionService
{
    public class STAApplicationContext : ApplicationContext
    {
        private IDeviceController _deviceController;
        private MenuViewController _menuViewController;
        private InputSerializer _inputSerializer;
        private InputLogger _inputLogger;
        private TuioServer _tuioServer;

        public STAApplicationContext()
        {
            var isDeviceMocked = false;
            if (!isDeviceMocked)
            {
                var touchOverlay = new TouchOverlay();
                _deviceController = (IDeviceController)touchOverlay;
                _inputSerializer = new InputSerializer(touchOverlay);
                _inputLogger = new InputLogger(touchOverlay);
                _tuioServer = new TuioServer(touchOverlay);
            }
            else
            {
                _deviceController = (IDeviceController)new DeviceMock();
            }

            _menuViewController = new MenuViewController(_deviceController);

            _deviceController.OnStateChanged += _menuViewController.OnStateChanged;
            _deviceController.Init();
            _deviceController.Start();
        }

        // Called from the Dispose method of the base class
        protected override void Dispose(bool disposing)
        {
            if (_inputSerializer != null)
            {
                _inputSerializer.Dispose();
                _inputSerializer = null;
            }
            if (_inputLogger != null)
            {
                _inputLogger.Dispose();
                _inputLogger = null;
            }
            if (_deviceController != null)
            {
                _deviceController.Terminate();
            }
            if ((_deviceController != null) && (_menuViewController != null))
            {
                _deviceController.OnStateChanged -= _menuViewController.OnStateChanged;
            }
            _deviceController = null;
            _menuViewController = null;
        }
    }
}
