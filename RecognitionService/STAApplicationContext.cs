using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Numerics;

using RecognitionService.Api;
using RecognitionService.Models;
using RecognitionService.Services;
using RecognitionService.Input.Touch;
using RecognitionService.Input.Tuio;

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
        private const string isDeviceMockedKey = "isDeviceMocked";
        private const string inputLoggingKey = "inputLogging";
        private const string inputSerializationKey = "inputSerialization";

        private const int _serverPort = 8080;

        private IDeviceController _deviceController;
        private MenuViewController _menuViewController;

        private IInputProvider _touchInputProvider;
        private InputSerializer _inputSerializer;
        private InputLogger _inputLogger;

        private ITuioInputProvider _tuioInputProvider;
        private TuioServer _tuioServer;

        private RecognitionServiceServer _wsServer;
        private TangibleMarkerController _tangibleMarkerController = new TangibleMarkerController();

        private JsonStorage<Settings> _settingsStorage = new JsonStorage<Settings>("settings");
        private Settings _globalSettings;

        private List<IDisposable> _disposeBag = new List<IDisposable>();

        private readonly Dictionary<string, bool> featureToggles = new Dictionary<string, bool>()
        {
            [isDeviceMockedKey] = false,
            [inputLoggingKey] = false,
            [inputSerializationKey] = true
        };

        public STAApplicationContext()
        {
            _globalSettings = _settingsStorage.Load();
            SetupServer();

            if (!featureToggles[isDeviceMockedKey])
            {
                var touchOverlay = (new TouchOverlay()).AddToDisposeBag(_disposeBag);
                _deviceController = (IDeviceController)touchOverlay;
                _touchInputProvider = (IInputProvider)touchOverlay;
            }
            else
            {
                var deviceMock = (new DeviceMock()).AddToDisposeBag(_disposeBag);
                _deviceController = (IDeviceController)deviceMock;
                _touchInputProvider = (IInputProvider)deviceMock;
            }

            if (featureToggles[inputSerializationKey])
            {
                _inputSerializer = (new InputSerializer(_touchInputProvider))
                    .AddToDisposeBag(_disposeBag);
            }

            if (featureToggles[inputLoggingKey])
            {
                _inputLogger = (new InputLogger(_touchInputProvider))
                    .AddToDisposeBag(_disposeBag);
            }

            _tuioInputProvider = (new TuioObjectController(_touchInputProvider, _tangibleMarkerController))
                .AddToDisposeBag(_disposeBag);

            _tuioServer = (new TuioServer(_tuioInputProvider))
                .AddToDisposeBag(_disposeBag);

            _menuViewController = (new MenuViewController(_deviceController))
                .AddToDisposeBag(_disposeBag);

            _deviceController.Init();
            _deviceController.Start();
        }

        private void SetupServer()
        {
            _wsServer = (new RecognitionServiceServer(_globalSettings.TuioServerPort))
                .AddToDisposeBag(_disposeBag);

            _wsServer.OnMarkerListRequested += _tangibleMarkerController.GetAllRegistredIds;
            _wsServer.OnRegisterMarkerRequested += (id, triangleInfo) =>
            {
                var width = _touchInputProvider.ScreenWidth;
                var heigth = _touchInputProvider.ScreenHeight;

                var absoluteA = new Vector2(triangleInfo.posA.X * width, triangleInfo.posA.Y * heigth);
                var absoluteB = new Vector2(triangleInfo.posB.X * width, triangleInfo.posB.Y * heigth);
                var absoluteC = new Vector2(triangleInfo.posC.X * width, triangleInfo.posC.Y * heigth);
                
                _tangibleMarkerController.RegisterMarkerWithId(id, (absoluteA, absoluteB, absoluteC));
            };
            _wsServer.OnUnregisterMarkerRequested += _tangibleMarkerController.UnregisterMarkerWithId;
        }

        // Called from the Dispose method of the base class
        protected override void Dispose(bool disposing)
        {
            _disposeBag.ForEach(disposable => disposable.Dispose());
        }
    }
}
