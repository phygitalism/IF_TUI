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

		// markerID - recognized marker
		private Dictionary<int, RecognizedTangibleMarker> previouslyRecognizedTangibles = new Dictionary<int, RecognizedTangibleMarker>();
		public Dictionary<int, int> markerTouchesIdToMarkersId = new Dictionary<int, int>();
		

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
				var validTouches = frame.ExtractValidTouches(markerTouchesIdToMarkersId);
				var markerTouches = frame.ExtractMarkerTouches(markerTouchesIdToMarkersId);
				//var activeMarkersIDs = registredTangibles.Where(marker => marker.State == RegistredTangibleMarker.MarkerState.Active).Select(marker => marker.Id).ToList();
				var passiveMarkers = registredTangibles.Where(marker => marker.State == RegistredTangibleMarker.MarkerState.Passive).ToList();
				
				var newRecognizedTangibles = _tangibleMarkerRecognizer.RecognizeTangibleMarkers(validTouches, passiveMarkers);
				AddRecognizedMarkersTouches(newRecognizedTangibles);
				var currentRecognizedTangibles = previouslyRecognizedTangibles;
				var allRecognizedTangibles = ConcatenateRecognizedTangibles(currentRecognizedTangibles, newRecognizedTangibles);
				

				RecognizedMarkersToActive(newRecognizedTangibles);
				//var activeMarkersIDs = updatedRegistredTangibles.Values.Where(marker => marker.State == RegistredTangibleMarker.MarkerState.Active).Select(marker => marker.Id).ToList();
				//RemoveLostMarkers(); ???
				var activeMarkers = DetermineMarkerState(allRecognizedTangibles.Values.ToList());
				previouslyRecognizedTangibles = activeMarkers;

				PrintMarkerStates(activeMarkers.Values.ToList());
				// TODO - split touches from objects

				var touchesWithRelativeCoords = frame.touches
					.Select(touch => touch.ToRelativeCoordinates(
						_inputProvider.ScreenWidth,
						_inputProvider.ScreenHeight
					)).ToList();

				var updatedActiveMarkers = UpdateTangibleMarkerPosition(activeMarkers, markerTouches);
				OnTuioInput?.Invoke(touchesWithRelativeCoords, updatedActiveMarkers);
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex);
			}
		}

		private List<RecognizedTangibleMarker> RecognizedMarkersToActive(List<RegistredTangibleMarker> registredTangibles,
			List<RecognizedTangibleMarker> newRecognizedTangibles)
		{
			foreach (var marker in newRecognizedTangibles)
			{
				registredTangiblesDictionary[marker.Id].State = RegistredTangibleMarker.MarkerState.Active;
			}

			var activeIds = registredTangiblesDictionary.Values.Where(marker =>
				marker.State == RegistredTangibleMarker.MarkerState.Active).Select(marker => marker.Id);

			var activeMarkers = previouslyRecognizedTangibles.Values.Where(marker => activeIds.Contains(marker.Id)).ToList();
			return activeMarkers;
		}
		
		//обновляем положение точек маркера
		private List<RecognizedTangibleMarker> UpdateTangibleMarkerPosition(Dictionary<int, RecognizedTangibleMarker> activeMarkers,
			List<TouchPoint> markerTouches)
		{
			var markerTouchesDictionary = markerTouches.ToDictionary(o => o.id);
			foreach (var touchId in markerTouchesIdToMarkersId.Keys)
			{
				int currentMarkerId = markerTouchesIdToMarkersId[touchId];
				TouchPoint currentTouch = markerTouchesDictionary[touchId];
				activeMarkers[currentMarkerId].UpdatePosition(currentTouch);
			}
			var tangiblesWithUpdatedCenters = activeMarkers.Values.ToList();
			tangiblesWithUpdatedCenters.ForEach(t => 
			{
				t.relativeCenter = new System.Numerics.Vector2(
                    t.Center.X / _inputProvider.ScreenWidth,
                    t.Center.Y / _inputProvider.ScreenHeight
                );
			});
			return tangiblesWithUpdatedCenters;
		}

		private void AddRecognizedMarkersTouches(List<RecognizedTangibleMarker> newRecognizedTangibles)
		{
			foreach (var marker in newRecognizedTangibles)
			{
				AddMarkerTouches(marker);
			}
		}
		
		private void AddMarkerTouches(RecognizedTangibleMarker recognizedMarker)
		{
			markerTouchesIdToMarkersId.Add(recognizedMarker.triangle.posA.id, recognizedMarker.Id);
			markerTouchesIdToMarkersId.Add(recognizedMarker.triangle.posB.id, recognizedMarker.Id);
			markerTouchesIdToMarkersId.Add(recognizedMarker.triangle.posC.id, recognizedMarker.Id);
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
            //if (uptedMarkerIds.Count > 0)
            //{
            //    Console.WriteLine($"Updated markers ids: {string.Join(" ", uptedMarkerIds)}");
            //}
            if (deletedMarkerIds.Count > 0)
            {
                Console.WriteLine($"Removed markers ids: {string.Join(" ", deletedMarkerIds)}");
            }
        }
         

		private Dictionary<int, RecognizedTangibleMarker> DetermineMarkerState(List<RecognizedTangibleMarker> recognizedTangibles)//, List<RegistredTangibleMarker> registredTangibles)
		{
			var currentRecognizedTangibles = new Dictionary<int, RecognizedTangibleMarker>();
			foreach (var tangible in recognizedTangibles)
			{
				if (previouslyRecognizedTangibles.ContainsKey(tangible.Id) && previouslyRecognizedTangibles[tangible.Id].Type != RecognizedTangibleMarker.ActionType.Removed)
				{
                    // Updated
                    tangible.Type = RecognizedTangibleMarker.ActionType.Updated;
                    currentRecognizedTangibles[tangible.Id] = tangible;
                }
				else
				{
					// Added
                    tangible.Type = RecognizedTangibleMarker.ActionType.Added;
                    currentRecognizedTangibles[tangible.Id] = tangible;
                }
			}

            try
            {
                var lookup = recognizedTangibles.ToDictionary(o => o.Id);
			    foreach (var tangible in previouslyRecognizedTangibles.Values)
			    {
				    if (!lookup.ContainsKey(tangible.Id) && previouslyRecognizedTangibles[tangible.Id].Type != RecognizedTangibleMarker.ActionType.Removed)
				    {
                        // Removed
                        tangible.Type = RecognizedTangibleMarker.ActionType.Removed;
                        
                        _tangibleMarkerController.Config.ChangeToPassive(tangible.Id);
                        
                        currentRecognizedTangibles[tangible.Id] = tangible;
				    }
			    }
            }
            catch (System.ArgumentException ex)
            {
                Console.WriteLine(ex);
                // TODO - the same marker was recognized from different triangle
            }
            return currentRecognizedTangibles;
		}

		public void Dispose()
		{
			_inputProvider.OnTouchesRecieved -= ProcessFrame;
		}
	}
}
