using System;
using System.Collections.Generic;

using RecognitionService.Models;

namespace RecognitionService
{
    public class TuioObjectController : ITuioInputProvider, IDisposable
    {
        private IInputProvider _inputProvider;
        private TangibleMarkerController _tangibleMarkerController;

        public event Action<List<TouchPoint>, List<RecognizedTangibleMarker>> OnTuioInput;

        public TuioObjectController(IInputProvider inputProvider, TangibleMarkerController tangibleMarkerController)
        {
            _inputProvider = inputProvider;
            _inputProvider.OnTouchesRecieved += ProcessFrame;
            _tangibleMarkerController = tangibleMarkerController;
        }

        private void ProcessFrame(TouchPointFrame frame)
        {

        }

        public void Dispose()
        {
            _inputProvider.OnTouchesRecieved -= ProcessFrame;
        }
    }
}
