﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using RecognitionService.Models;
using RecognitionService.Api;

namespace RecognitionService
{
    public static class DisposebleExtensions
    {
        public static T AddToDisposeBag<T>(this T disposable, List<IDisposable> disposeBag) where T : IDisposable
        {
            disposeBag.Add(disposable);
            return disposable;
        }
    }

    public class STAApplicationContext : ApplicationContext
    {
        private const int _serverPort = 8080;

        private IDeviceController _deviceController;
        private MenuViewController _menuViewController;

        private IInputProvider _touchInputProvider;
        private InputSerializer _inputSerializer;
        private InputLogger _inputLogger;

        private ITuioInputProvider _tuioInputProvider;
        private TuioServer _tuioServer;

        private TouchPointFrameGenerator _touchPointFrameGenerator;

        private RecognitionServiceServer _wsServer;
        private TangibleMarkerController _tangibleMarkerController = new TangibleMarkerController();

        private List<IDisposable> _disposeBag = new List<IDisposable>();


        public STAApplicationContext()
        {
            SetupServer();

            var isDeviceMocked = false;
            if (!isDeviceMocked)
            {
                var touchOverlay = (new TouchOverlay()).AddToDisposeBag(_disposeBag);
                _deviceController = (IDeviceController)touchOverlay;
                _touchInputProvider = (IInputProvider)touchOverlay;

                _inputSerializer = (new InputSerializer(_touchInputProvider))
                    .AddToDisposeBag(_disposeBag);

                _inputLogger = (new InputLogger(_touchInputProvider))
                    .AddToDisposeBag(_disposeBag);

                _tuioInputProvider = (new TuioObjectController(_touchInputProvider, _tangibleMarkerController))
                    .AddToDisposeBag(_disposeBag);

                _tuioServer = (new TuioServer(_tuioInputProvider))
                    .AddToDisposeBag(_disposeBag);
            }
            else
            {
                var deviceMock = (new DeviceMock()).AddToDisposeBag(_disposeBag);
                _deviceController = (IDeviceController)deviceMock;
            }

            _menuViewController = new MenuViewController(_deviceController);

            _deviceController.OnStateChanged += _menuViewController.OnStateChanged;
            _deviceController.Init();
            _deviceController.Start();
        }

        private void SetupServer()
        {
            _wsServer = (new RecognitionServiceServer(_serverPort))
                .AddToDisposeBag(_disposeBag);

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
            _disposeBag.ForEach(disposable => disposable.Dispose());

            if ((_deviceController != null) && (_menuViewController != null))
            {
                _deviceController.OnStateChanged -= _menuViewController.OnStateChanged;
            }
            _deviceController = null;
            _menuViewController = null;
        }
    }
}
