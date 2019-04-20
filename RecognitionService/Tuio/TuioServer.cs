using System;
using System.Collections.Generic;

using RecognitionService.Models;
using RecognitionService.Services;
using RecognitionService.Input.Tuio;
using RecognitionService.Tuio;
using RecognitionService.Tuio.Entities;

using log4net;
using log4net.Config;

namespace RecognitionService
{
    public class TuioServer : IDisposable
    {
        private static readonly ILog Logger = LogManager.GetLogger(typeof(TuioServer));
        public const string defaultIPAddress = "127.0.0.1";
        public const int defaultTuioPort = 3334;

        private ITuioInputProvider _tuioInputProvider;
        private TUIOTransmitter _tuioTransmitter;
        private Dictionary<int, TUIOCursor> cursors = new Dictionary<int, TUIOCursor>();
        private Dictionary<int, TUIOObject> objects = new Dictionary<int, TUIOObject>();

        private JsonStorage<Settings> _settingsStorage = new JsonStorage<Settings>("settings");
        private Settings _globalSettings;

        public TuioServer(ITuioInputProvider tuioInputProvider)
        {
            _tuioInputProvider = tuioInputProvider;
            _tuioInputProvider.OnTuioInput += Send;

            int tuioPort;
            try
            {
                _globalSettings = _settingsStorage.Load();
                tuioPort = _globalSettings.TuioServerPort;
            }
            catch (Exception e)
            {
                Logger.Warn(e);
                tuioPort = defaultTuioPort;
            }
            _tuioTransmitter = new TUIOTransmitter(defaultIPAddress, tuioPort);
            _tuioTransmitter.Connect();
        }

        private void Send(List<TouchPoint> touches, List<RecognizedTangibleMarker> tangibles)
        {
            ProcessCursors(touches);
            ProcessObjects(tangibles);

            try
            {
                _tuioTransmitter.Send();
            }
            catch (SendTuioBundleException ex)
            {
                Logger.Error(ex);
            }
        }

        private void ProcessCursors(List<TouchPoint> touches)
        {
            foreach (var tp in touches)
            {
                switch (tp.Type)
                {
                    case TouchPoint.ActionType.Down:
                        var cursorToAdd = new TUIOCursor(_tuioTransmitter.NextSessionId(), tp.Position.X, tp.Position.Y, 0f, 0f, 0f);
                        cursors[tp.Id] = cursorToAdd;
                        _tuioTransmitter.Add(cursorToAdd);
                        break;
                    case TouchPoint.ActionType.Move:
                        cursors[tp.Id]?.Update(tp.Position.X, tp.Position.Y, 0f, 0f, 0f);
                        break;
                    case TouchPoint.ActionType.Up:
                        var cursorToRemove = cursors[tp.Id];
                        _tuioTransmitter.Remove(cursorToRemove);
                        cursors.Remove(tp.Id);
                        break;
                    default:
                        Logger.Error($"ERROR: unkown touchpoint action type");
                        break;
                }
            }
        }

        private void ProcessObjects(List<RecognizedTangibleMarker> tangibles)
        {
            foreach (var tangible in tangibles)
            {
                switch (tangible.Type)
                {
                    case RecognizedTangibleMarker.ActionType.Added:
						Logger.Info($"Tuio object added {tangible.Id}");
                        var objectToAdd = new TUIOObject(_tuioTransmitter.NextSessionId(), tangible.Id, tangible.RelativeCenter.X, tangible.RelativeCenter.Y, tangible.RotationAngle, 0f, 0f, 0f, 0f, 0f);
                        objects[tangible.Id] = objectToAdd;
                        _tuioTransmitter.Add(objectToAdd);
                        break;
                    case RecognizedTangibleMarker.ActionType.Updated:
                        if (objects.ContainsKey(tangible.Id))
                        {
                            objects[tangible.Id]?.Update(tangible.RelativeCenter.X, tangible.RelativeCenter.Y,
                                tangible.RotationAngle, 0f, 0f, 0f, 0f, 0f);
                        }
                        break;
                    case RecognizedTangibleMarker.ActionType.Unstable:
                        Logger.Info($"Tuio object unstable {tangible.Id}");
                        break;
                    case RecognizedTangibleMarker.ActionType.Removed:
                        if (objects.ContainsKey(tangible.Id))
                        {
                            Logger.Info($"Tuio object Removed {tangible.Id}");
                            var objectToRemove = objects[tangible.Id];
                            _tuioTransmitter.Remove(objectToRemove);
                            objects.Remove(tangible.Id);
                        }
                        break;
                    default:
                        Logger.Error($"ERROR: unkown tangible action type");
                        break;
                }
            }
        }

        public void Dispose()
        {
            _tuioInputProvider.OnTuioInput -= Send;
            _tuioTransmitter.Close();
        }
    }
}
