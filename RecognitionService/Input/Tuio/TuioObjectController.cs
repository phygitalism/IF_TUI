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
			try
			{
				var registredTangibles = _tangibleMarkerController.Config.registredTangibles;
				var recognizedTangibles = _tangibleMarkerRecognizer.RecognizeTangibleMarkers(frame.touches, registredTangibles);
				var currentRecognizedTangibles = DetermineMarkerState(recognizedTangibles);
				previouslyRecognizedTangibles = currentRecognizedTangibles;

                PrintMarkerStates(currentRecognizedTangibles.Values.ToList());
                // TODO - split touches from objects

                OnTuioInput?.Invoke(frame.touches, currentRecognizedTangibles.Values.ToList());
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex);
			}

		}

        private void PrintMarkerStates(List<RecognizedTangibleMarker> recognizedTangibles)
        {
            var addedMarkerIds = recognizedTangibles.Where(marker => marker.Type == RecognizedTangibleMarker.ActionType.Added).Select(marker => marker.Id).ToList();
            var uptedMarkerIds = recognizedTangibles.Where(marker => marker.Type == RecognizedTangibleMarker.ActionType.Updated).Select(marker => marker.Id).ToList();
            var deletedMarkerIds = recognizedTangibles.Where(marker => marker.Type == RecognizedTangibleMarker.ActionType.Removed).Select(marker => marker.Id).ToList();

            if (addedMarkerIds.Count > 0)
            {
                Console.WriteLine($"Added markers ids: {string.Join(" ", addedMarkerIds)}");
            }
            if (uptedMarkerIds.Count > 0)
            {
                Console.WriteLine($"Updated markers ids: {string.Join(" ", uptedMarkerIds)}");
            }
            if (deletedMarkerIds.Count > 0)
            {
                Console.WriteLine($"Removed markers ids: {string.Join(" ", deletedMarkerIds)}");
            }
        }

		private Dictionary<int, RecognizedTangibleMarker> DetermineMarkerState(List<RecognizedTangibleMarker> recognizedTangibles)
		{
			var currentRecognizedTangibles = new Dictionary<int, RecognizedTangibleMarker>();
			foreach (var tangible in recognizedTangibles)
			{
				if (previouslyRecognizedTangibles.ContainsKey(tangible.Id) && previouslyRecognizedTangibles[tangible.Id].Type != RecognizedTangibleMarker.ActionType.Removed)
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
				if (!lookup.ContainsKey(tangibleId) && previouslyRecognizedTangibles[tangibleId].Type != RecognizedTangibleMarker.ActionType.Removed)
				{
					// Removed
					var tuioObj = previouslyRecognizedTangibles[tangibleId];
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
