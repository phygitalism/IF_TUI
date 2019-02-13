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
			try
			{
				var registredTangibles = _tangibleMarkerController.Config.registredTangibles;
				_activityController.ProcessTouchPointFrame(frame, registredTangibles);
				
				var validTouches = _activityController.ValidTouches;
				//мигать не будет тк если маркер пропал его уже удалили из recognizedMarkers
				var passiveMarkers = _activityController.PassiveMarkers;
				//новые распознанные автоматически имеют тип Added
				var newRecognizedTangibles = _tangibleMarkerRecognizer.RecognizeTangibleMarkers(validTouches, passiveMarkers);
				_activityController.AddRecognizedMarkers(newRecognizedTangibles);
				
				var recognizedMarkers = _activityController.ActiveMarkers;
				PrintMarkerStates(recognizedMarkers);

				var touchesWithRelativeCoords = frame.touches
					.Select(touch => touch.ToRelativeCoordinates(
						_inputProvider.ScreenWidth,
						_inputProvider.ScreenHeight
					)).ToList();
				
				
				OnTuioInput?.Invoke(touchesWithRelativeCoords, _activityController.ActiveMarkers);

				//теперь можно удалить дохлые маркеры
				_activityController.RemoveLostMarkers();
				//в конце в recognizedMarkers останутся только активные (Added and Updated)
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
