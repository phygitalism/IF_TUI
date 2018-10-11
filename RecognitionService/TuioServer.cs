using System;
using System.Collections.Generic;

using RecognitionService.Models;
using RecognitionService.Tuio;
using RecognitionService.Tuio.Entities;

namespace RecognitionService
{
    public class TuioServer : IDisposable
    {
        private IInputProvider _inputProvider;

        public const string defaultIPAddress = "127.0.0.1";
        public const int defaultPort = 3333;

        private TUIOTransmitter _tuioTransmitter;
        private Dictionary<int, TUIOCursor> cursors = new Dictionary<int, TUIOCursor>();

        public TuioServer(IInputProvider inputProvider)
        {
            _inputProvider = inputProvider;
            _inputProvider.OnTouchesRecieved += Send;

            _tuioTransmitter = new TUIOTransmitter(defaultIPAddress, defaultPort);
            _tuioTransmitter.Connect();
        }

        private void Send(TouchPointFrame frame)
        {
            foreach (var tp in frame.touches)
            {
                switch (tp.type)
                {
                    case TouchPoint.ActionType.Down:
                        var cursorToAdd = new TUIOCursor(_tuioTransmitter.NextSessionId(), tp.Position.X, tp.Position.Y, 0f, 0f, 0f);
                        cursors[tp.id] = cursorToAdd;
                        _tuioTransmitter.Add(cursorToAdd);
                        break;
                    case TouchPoint.ActionType.Move:
                        cursors[tp.id]?.Update(tp.Position.X, tp.Position.Y, 0f, 0f, 0f);
                        break;
                    case TouchPoint.ActionType.Up:
                        var cursorToRemove = cursors[tp.id];
                        _tuioTransmitter.Remove(cursorToRemove);
                        cursors.Remove(tp.id);
                        break;
                    default:
                        Console.WriteLine($"ERROR: unkown touchpoint action type");
                        break;
                }
            }

            try
            {
                Console.WriteLine("SEND");
                _tuioTransmitter.Send();
            }
            catch (SendTuioBundleException ex)
            {
                Console.WriteLine(ex);
            }
        }

        public void Dispose()
        {
            _tuioTransmitter.Close();
            _inputProvider.OnTouchesRecieved -= Send;
        }
    }
}
