using System;
using System.Collections.Generic;

using RecognitionService.Models;
using RecognitionService.Services;
using RecognitionService.Input.Tuio;
using RecognitionService.Tuio;
using RecognitionService.Tuio.Entities;

namespace RecognitionService
{
    public class TuioServer : IDisposable
    {
        public const string defaultIPAddress = "127.0.0.1";
        public const int defaultTuioPort = 3333;

        private ITuioInputProvider _tuioInputProvider;
        private TUIOTransmitter _tuioTransmitter;
        private Dictionary<int, TUIOCursor> cursors = new Dictionary<int, TUIOCursor>();
        private Dictionary<int, TUIOObject> objects = new Dictionary<int, TUIOObject>();

        private JsonStorage<Settings> _settingsStorage = new JsonStorage<Settings>("settings");
        private Settings _globalSettings;

        public TuioServer(ITuioInputProvider tuioInputProvider)
        {
            _globalSettings = _settingsStorage.Load();
            _tuioInputProvider = tuioInputProvider;
            _tuioInputProvider.OnTuioInput += Send;

            _tuioTransmitter = new TUIOTransmitter(defaultIPAddress, _globalSettings.TuioServerPort);
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
                Console.WriteLine(ex);
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
                        Console.WriteLine($"ERROR: unkown touchpoint action type");
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
						Console.WriteLine($"Tuio object added {tangible.Id}");
                        var objectToAdd = new TUIOObject(_tuioTransmitter.NextSessionId(), tangible.Id, tangible.RelativeCenter.X, tangible.RelativeCenter.Y, tangible.RotationAngle, 0f, 0f, 0f, 0f, 0f);
                        objects[tangible.Id] = objectToAdd;
                        _tuioTransmitter.Add(objectToAdd);
                        break;
                    case RecognizedTangibleMarker.ActionType.Updated:
						objects[tangible.Id]?.Update(tangible.RelativeCenter.X, tangible.RelativeCenter.Y, tangible.RotationAngle, 0f, 0f, 0f, 0f, 0f);
                        break;
                    case RecognizedTangibleMarker.ActionType.Unstable:
                        Console.WriteLine($"Tuio object unstable {tangible.Id}");
                        break;
                    case RecognizedTangibleMarker.ActionType.Removed:
						Console.WriteLine($"Tuio object Removed {tangible.Id}");
						var objectToRemove = objects[tangible.Id];
                        _tuioTransmitter.Remove(objectToRemove);
                        objects.Remove(tangible.Id);
                        break;
                    default:
                        Console.WriteLine($"ERROR: unkown tangible action type");
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
