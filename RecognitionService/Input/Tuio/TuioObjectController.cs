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
		
		private MarkerActivityController _activityController = new MarkerActivityController();
		private ITangibleMarkerRecognizer _recognizer = new TangibleMarkerRecognizer();

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
				_activityController.ProcessTouchPointFrame(frame, registredTangibles);

				var passiveTouches = _activityController.PassiveTouches;
				var passiveMarkers = _activityController.PassiveMarkers;
				var unstableMarkers = _activityController.UnstableMarkers;

				//новые распознанные автоматически имеют тип Added
				var newRecognizedTangibles = _recognizer.RecognizeTangibleMarkers(passiveTouches, passiveMarkers.Concat(unstableMarkers).ToList());
				_activityController.AddRecognizedMarkers(newRecognizedTangibles);

				//passiveTouches = _activityController.PassiveTouches;
				//var unstableMarkers = _activityController.UnstableMarkers;
				//var unstableRecognizedTangibles = _recognizer.RecognizeTangibleMarkers(passiveTouches, unstableMarkers);
				
				var allActiveMarkers = _activityController.ActiveMarkers;
				PrintMarkerStates(allActiveMarkers);
				
				//приводим информацию о центре маркеров к относительным координатам
				allActiveMarkers.ForEach(t => 
				{
					t.RelativeCenter = new System.Numerics.Vector2(
						t.Center.X / _inputProvider.ScreenWidth,
						t.Center.Y / _inputProvider.ScreenHeight
					);
				});

				//приводим информацию о тачах к относительным координатам
				var touchesWithRelativeCoords = frame.Touches
					.Select(touch => touch.ToRelativeCoordinates(
						_inputProvider.ScreenWidth,
						_inputProvider.ScreenHeight
					)).ToList();
				
				
				OnTuioInput?.Invoke(touchesWithRelativeCoords, allActiveMarkers);

				//теперь можно удалить маркеры с меткой Remove
				//в конце в recognizedMarkers останутся только активные (Added and Updated)
				_activityController.RemoveLostMarkers();
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
            //if (uptedMarkerIds.Count > 0)
            //{
            //    Console.WriteLine($"Updated markers ids: {string.Join(" ", uptedMarkerIds)}");
            //}
            if (deletedMarkerIds.Count > 0)
            {
                Console.WriteLine($"Removed markers ids: {string.Join(" ", deletedMarkerIds)}");
            }
        }
   
		public void Dispose()
		{
			_inputProvider.OnTouchesRecieved -= ProcessFrame;
		}
	}
}
