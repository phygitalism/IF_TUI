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
		//private List<RegistredTangibleMarker> updatedRegisteredMarkers = new List<RegistredTangibleMarker>(); 

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
				//updatedRegisteredMarkers = updatedRegisteredMarkers();
				var recognizedTangibles = _tangibleMarkerRecognizer.RecognizeTangibleMarkers(frame.touches, registredTangibles);
				var currentRecognizedTangibles = DetermineMarkerState(recognizedTangibles);
				previouslyRecognizedTangibles = currentRecognizedTangibles;

                PrintMarkerStates(currentRecognizedTangibles.Values.ToList());
                // TODO - split touches from objects

				var touchesWithRelativeCoords = frame.touches
					.Select(touch => touch.ToRelativeCoordinates(
						_inputProvider.ScreenWidth,
						_inputProvider.ScreenHeight
					)).ToList();
				
				var tangiblesWithUpdatedCenters = currentRecognizedTangibles.Values.ToList();
				tangiblesWithUpdatedCenters.ForEach(t => 
				{
					t.relativeCenter = new System.Numerics.Vector2(
                        t.Center.X / _inputProvider.ScreenWidth,
                        t.Center.Y / _inputProvider.ScreenHeight
                    );
				});

				OnTuioInput?.Invoke(touchesWithRelativeCoords, tangiblesWithUpdatedCenters);
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex);
			}

		}

		private List<RegistredTangibleMarker> FilterPassiveRegistredMarkers(List<RegistredTangibleMarker> registredTangibles)
		{
			/*previouslyRecognizedTangibles.ToList().Sort((id1, id2) => id1.Key>=id2.Key? 1: -1);
			for (int i = 0; i < registredTangibles.Count; i++)
			{
				if (previouslyRecognizedTangibles.Keys.ToList().BinarySearch(registredTangibles[i].Id))
				{
					registredTangibles[i].State = RegistredTangibleMarker.MarkerState.Active;
				}
				else
				{
					registredTangibles[i].State = RegistredTangibleMarker.MarkerState.Passive;
				}
			}*/
			//var intersection = registredTangibles.Where(marker => marker.IdIntersect(previouslyRecognizedTangibles.Keys));
			
			//var intersection = registredTangibles.SelectMany(reg => previouslyRecognizedTangibles.Where(rec => rec.Key == reg.Id)).ToList();
			var intersection = previouslyRecognizedTangibles.SelectMany(rec => registredTangibles.Where(reg => reg.Id == rec.Key)).ToList();
			foreach (var tangible in intersection)
			{
				tangible.State = RegistredTangibleMarker.MarkerState.Active;
			}
			var passiveMarkers = registredTangibles.Where(marker => marker.State == RegistredTangibleMarker.MarkerState.Passive).ToList();
			return passiveMarkers;
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

		private Dictionary<int, RecognizedTangibleMarker> DetermineMarkerState(List<RecognizedTangibleMarker> recognizedTangibles)
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
