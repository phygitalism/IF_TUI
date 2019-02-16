using System;

using RecognitionService.Models;
using RecognitionService.Input.Touch;

namespace RecognitionService
{
    public class InputLogger : IDisposable
    {
        private IInputProvider _inputProvider;

        public InputLogger(IInputProvider inputProvider)
        {
            _inputProvider = inputProvider;
            _inputProvider.OnTouchesRecieved += Log;
        }

        private void Log(TouchPointFrame frame)
        {
            foreach (var tp in frame.touches)
            {
                Console.WriteLine($"  point {tp.Id} come at ({tp.Position.X},{tp.Position.Y}) width:{tp.Acceleration.X} height:{tp.Acceleration.Y}");
            }
        }

        public void Dispose()
        {
            _inputProvider.OnTouchesRecieved -= Log;
        }
    }
}
