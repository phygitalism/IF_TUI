using System;
using System.Linq;
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

		private Dictionary<int, RecognizedTangibleMarker> previouslyRecognizedTangibles = new Dictionary<int, RecognizedTangibleMarker>();

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

			var currentRecognizedTangibles = DetermineMarkerState(recognizedTangibles);
			previouslyRecognizedTangibles = currentRecognizedTangibles;

			// TODO - split touches from objects

			OnTuioInput?.Invoke(frame.touches, currentRecognizedTangibles.Values.ToList());
		}

		private Dictionary<int, RecognizedTangibleMarker> DetermineMarkerState(List<RecognizedTangibleMarker> recognizedTangibles)
		{
			var currentRecognizedTangibles = new Dictionary<int, RecognizedTangibleMarker>();
			foreach (var tangible in recognizedTangibles)
			{
				if (previouslyRecognizedTangibles.ContainsKey(tangible.Id))
				{
					// Updated
					var tuioObj = previouslyRecognizedTangibles[tangible.Id];
					tuioObj.Type = RecognizedTangibleMarker.ActionType.Updated;
					currentRecognizedTangibles[tuioObj.Id] = tuioObj;
				}
				else
				{
					// Added
					var tuioObj = tangible;
					tuioObj.Type = RecognizedTangibleMarker.ActionType.Added;
					currentRecognizedTangibles[tuioObj.Id] = tuioObj;
				}
			}

			var lookup = recognizedTangibles.ToDictionary(o => o.Id);
			foreach (var tangibleId in previouslyRecognizedTangibles.Keys)
			{
				if (!lookup.ContainsKey(tangibleId))
				{
					// Removed
					var tuioObj = lookup[tangibleId];
					tuioObj.Type = RecognizedTangibleMarker.ActionType.Removed;
					currentRecognizedTangibles[tuioObj.Id] = tuioObj;
				}
			}

			return currentRecognizedTangibles;
		}

		public void Dispose()
		{
			_inputProvider.OnTouchesRecieved -= ProcessFrame;
		}
	}
}
