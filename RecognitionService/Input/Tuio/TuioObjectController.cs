using System;
using System.Collections.Generic;

using RecognitionService.Models;
using RecognitionService.Recognition;
using RecognitionService.Input.Touch;

namespace RecognitionService.Input.Tuio
{
	public class TuioObjectController : ITuioInputProvider, IDisposable
	{
		private IInputProvider _inputProvider;
		private TangibleMarkerController _tangibleMarkerController;
		private ITangibleMarkerRecognizer _tangibleMarkerRecognizer = new TangibleMarkerRecognizer();

		public event Action<List<TouchPoint>, List<RecognizedTangibleMarker>> OnTuioInput;

		public TuioObjectController(IInputProvider inputProvider, TangibleMarkerController tangibleMarkerController)
		{
			_inputProvider = inputProvider;
			_inputProvider.OnTouchesRecieved += ProcessFrame;
			_tangibleMarkerController = tangibleMarkerController;
		}

		private void ProcessFrame(TouchPointFrame frame)
		{
			var registredTangibles = _tangibleMarkerController.Config.registredTangibles;
			var recognizedTangibles = _tangibleMarkerRecognizer.RecognizeTangibleMarkers(frame.touches, registredTangibles);
			
			// TODO - detect previously recognized tangibles and tangibles that disappeared

			// TODO - split touches from objects

			OnTuioInput?.Invoke(frame.touches, recognizedTangibles);
		}

		public void Dispose()
		{
			_inputProvider.OnTouchesRecieved -= ProcessFrame;
		}
	}
}
