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
		private Dictionary<int, RecognizedTangibleMarker> recognizedMarkers = new Dictionary<int, RecognizedTangibleMarker>();
		//private Dictionary<int, HashSet<int>> markerIdtoMarkerTouchesIds = new Dictionary<int, HashSet<int>>();
		private Dictionary<int, int> markerTouchesIdToMarkersId = new Dictionary<int, int>();

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
				//recognizedMarkers - и есть активные маркеры (только Added and Updated)
				var registredTangibles = _tangibleMarkerController.Config.registredTangibles;
				var lostTouches = ExtractLostTouches(frame.touches);
				var markerTouches = ExtractMarkerTouches(frame.touches);
				var validTouches = ExtractValidTouches(frame.touches);
				//мигать не будет тк если маркер пропал его уже удалили из recognizedMarkers
				var passiveMarkers = registredTangibles.Where(reg => !recognizedMarkers.ContainsKey(reg.Id)).ToList();
				//новые распознанные автоматически имеют тип Added
				var newRecognizedTangibles = _tangibleMarkerRecognizer.RecognizeTangibleMarkers(validTouches, passiveMarkers);
				
				//обновляем положение распознанных маркеров, для Added обновляется только центр
				UpdateActiveMarkers(markerTouches);
				AddRecognizedMarkersTouches(newRecognizedTangibles);
				//помечаем удаленные маркеры как Removed и удаляем их тачи из словаря
				MarkLostMarkers(lostTouches);
				//соединяем все равспознанные тачи с новыми
				recognizedMarkers = recognizedMarkers.Concat(newRecognizedTangibles
					.ToDictionary(x => x.Id, x => x))
					.ToDictionary(x => x.Key, x => x.Value);

				PrintMarkerStates(recognizedMarkers.Values.ToList());
				// TODO - split touches from objects

				var touchesWithRelativeCoords = frame.touches
					.Select(touch => touch.ToRelativeCoordinates(
						_inputProvider.ScreenWidth,
						_inputProvider.ScreenHeight
					)).ToList();
				
				
				OnTuioInput?.Invoke(touchesWithRelativeCoords, recognizedMarkers.Values.ToList());
				//теперь можно удалить дохлые маркеры
				RemoveLostMarkers();
				//в конце в recognizedMarkers останутся только активные (Added and Updated)
			}
			catch (System.Exception ex)
			{
				Console.WriteLine(ex);
			}
		}
		
		public List<TouchPoint> ExtractValidTouches(List<TouchPoint> touches)
		{
			//точки которые не входят в зареганные 
			var validTouches = touches.Where(t => !markerTouchesIdToMarkersId.Keys.Contains(t.id)).ToList(); 
			return validTouches;
		}
        
		public List<TouchPoint> ExtractMarkerTouches(List<TouchPoint> touches)
		{
			//точки которые входят в зареганные
			var markerTouches = touches.Where(t => markerTouchesIdToMarkersId.Keys.Contains(t.id)).ToList(); 
			return markerTouches;
		}

		public List<TouchPoint> ExtractLostTouches(List<TouchPoint> touches)
		{
			//точки которые пропадут
			var lostTouches = touches.Where(t => t.type == TouchPoint.ActionType.Up).ToList();
			return lostTouches;
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
		private void MarkLostMarkers(List<TouchPoint> lostTouches)
		{
			var lostTouchesDictionary = lostTouches.ToDictionary(o => o.id);
			foreach (var lostTouchId in lostTouchesDictionary.Keys)
			{
				if (markerTouchesIdToMarkersId.ContainsKey(lostTouchId))
				{
					var lostMarkerId = markerTouchesIdToMarkersId[lostTouchId];
					RemoveLostMarkerTouches(lostMarkerId);
					recognizedMarkers[lostMarkerId].Type = RecognizedTangibleMarker.ActionType.Removed;
				}
			}
		}
		private void RemoveLostMarkerTouches(int lostMarkerId)
		{
			markerTouchesIdToMarkersId.Remove(recognizedMarkers[lostMarkerId].triangle.posA.id);
			markerTouchesIdToMarkersId.Remove(recognizedMarkers[lostMarkerId].triangle.posB.id);
			markerTouchesIdToMarkersId.Remove(recognizedMarkers[lostMarkerId].triangle.posC.id);
		}
		
		//обновляем положение точек маркера
		private void UpdateActiveMarkers(List<TouchPoint> markerTouches)
		{
			Dictionary<int, TouchPoint> markerTouchesDictionary = markerTouches.ToDictionary(o => o.id);
			foreach (var touchId in markerTouchesIdToMarkersId.Keys)
			{
				int currentMarkerId = markerTouchesIdToMarkersId[touchId];
				TouchPoint currentTouch = markerTouchesDictionary[touchId];
				recognizedMarkers[currentMarkerId].UpdatePosition(currentTouch);
				recognizedMarkers[currentMarkerId].Type = RecognizedTangibleMarker.ActionType.Updated;
			}
			//а центр обновляется вапще у всех
			recognizedMarkers.Values.ToList().ForEach(t => 
			{
				t.relativeCenter = new System.Numerics.Vector2(
                    t.Center.X / _inputProvider.ScreenWidth,
                    t.Center.Y / _inputProvider.ScreenHeight
                );
			});
		}

		private void RemoveLostMarkers()
		{
			var removedMarkersIds = recognizedMarkers.Values.Where(marker => 
					marker.Type == RecognizedTangibleMarker.ActionType.Removed)
					.Select(marker => marker.Id);
			foreach (var removedId in removedMarkersIds)
			{
				recognizedMarkers.Remove(removedId);
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
         
		/*
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
		}*/

		public void Dispose()
		{
			_inputProvider.OnTouchesRecieved -= ProcessFrame;
		}
	}
}
