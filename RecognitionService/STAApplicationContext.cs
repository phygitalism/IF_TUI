using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RecognitionService.Models;
using RecognitionService.Api;

namespace RecognitionService
{
    public class STAApplicationContext : ApplicationContext
    {
        private const int _serverPort = 8080;

        private IDeviceController _deviceController;
        private MenuViewController _menuViewController;
        private InputSerializer _inputSerializer;
        private InputLogger _inputLogger;
        private TuioServer _tuioServer;

        private TouchPointFrameGenerator _touchPointFrameGenerator;

        private RecognitionServiceServer _wsServer;

        private TangibleMarkerController _tangibleMarkerController = new TangibleMarkerController();


        public STAApplicationContext()
        {
            SetupServer();

            var isDeviceMocked = false;
            if (!isDeviceMocked)
            {
                var touchOverlay = new TouchOverlay();
                _deviceController = (IDeviceController)touchOverlay;

                _inputSerializer = new InputSerializer(touchOverlay);
                _inputLogger = new InputLogger(touchOverlay);
                _tuioServer = new TuioServer(touchOverlay);
                //_touchPointFrameGenerator = new TouchPointFrameGenerator();
                //_tuioServer = new TuioServer(_touchPointFrameGenerator);
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

        private void SetupServer()
        {
            _wsServer = new RecognitionServiceServer(_serverPort);
            _wsServer.OnMarkerListRequested += _tangibleMarkerController.GetAllRegistredIds;
            _wsServer.OnRegisterMarkerRequested += (id, triangleInfo) =>
            {
                var triangle = new Models.Triangle(triangleInfo.posA, triangleInfo.posB, triangleInfo.posC);
                _tangibleMarkerController.RegisterMarkerWithId(triangle, id);
            };
            _wsServer.OnUnregisterMarkerRequested += _tangibleMarkerController.UnregisterMarkerWithId;
        }

        // Called from the Dispose method of the base class
        protected override void Dispose(bool disposing)
        {
            if (_wsServer != null)
            {
                _wsServer.Dispose();
                _wsServer = null;
            }
            if (_tangibleMarkerController != null)
            {
                _tangibleMarkerController.Dispose();
                _tangibleMarkerController = null;
            }
            if (_touchPointFrameGenerator != null)
            {
                _touchPointFrameGenerator.Dispose();
                _touchPointFrameGenerator = null;
            }
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
