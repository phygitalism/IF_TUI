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

        public STAApplicationContext()
        {
            var isDeviceMocked = false;
            _deviceController = isDeviceMocked ? (IDeviceController)new DeviceMock() : (IDeviceController)new TouchOverlay();
            _menuViewController = new MenuViewController(_deviceController);

            _deviceController.OnStateChanged += _menuViewController.OnStateChanged;
            _deviceController.Init();
            _deviceController.Start();
        }

        // Called from the Dispose method of the base class
        protected override void Dispose(bool disposing)
        {
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
