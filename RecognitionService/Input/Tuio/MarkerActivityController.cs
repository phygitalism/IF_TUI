using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

using RecognitionService.Models;

namespace RecognitionService.Input.Tuio
{
    public class MarkerActivityController
    {
        // recognizedMarkers - и есть активные маркеры (только Added and Updated)
        private Dictionary<int, RecognizedTangibleMarker> _recognizedMarkers = new Dictionary<int, RecognizedTangibleMarker>();
        private Dictionary<int, HashSet<TouchPoint>> markerTouches = new Dictionary<int, HashSet<TouchPoint>>();
        private HashSet<TouchPoint> touches = new HashSet<TouchPoint>();
        private List<RegistredTangibleMarker> _registredTangibles = new List<RegistredTangibleMarker>();
        
        public List<TouchPoint> LostTouches
        {
            get {

                //точки которые пропадут
                return touches.Where(t => t.type == TouchPoint.ActionType.Up).ToList();
            }
        }

        public List<TouchPoint> MarkerTouches
        {
            get
            {
                //точки которые входят в зареганные
                return markerTouches.Values
                    .Select(touchePoints => touchePoints.ToList())
                    .SelectMany(touchPoint => touchPoint).ToList();
            }

        }

        public List<TouchPoint> ValidTouches
        {
            get
            {
                //точки которые не входят в зареганные
                var markerTouchIds = MarkerTouches.Select(touch => touch.id);
                return touches.Where(touch => !markerTouchIds.Contains(touch.id)).ToList();
            }
        }

        public List<RegistredTangibleMarker> PassiveMarkers
        {
            get
            {
                return _registredTangibles.Where(reg => !markerTouches.ContainsKey(reg.Id)).ToList();
            }
        }
        
        public List<RecognizedTangibleMarker> ActiveMarkers
        {
            get
            {    // TODO
//                return _registredTangibles.Where(reg => !markerTouches.ContainsKey(reg.Id)).ToList();
                return new List<RecognizedTangibleMarker>();
            }
            
        }
        
        public MarkerActivityController(){ }
        
        public void ProcessTouchPointFrame(TouchPointFrame frame, List<RegistredTangibleMarker> registredMarkers)
        {
            touches = new HashSet<TouchPoint>(frame.touches);
            _registredTangibles = registredMarkers;
            // update and remove
            //обновляем положение распознанных маркеров, для Added обновляется только центр
            UpdateRecognizedMarkers(markerTouches); // все Addded пометим Updated
            //помечаем удаленные маркеры как Removed и удаляем их тачи из словаря
            MarkLostMarkers(lostTouches);
        }
        
        public void AddRecognizedMarkers(List<RecognizedTangibleMarker> newRecognizedTangibles)
        {
            //
            foreach (var marker in newRecognizedTangibles)
            {
                AddMarkerTouches(marker);
            }
            //соединяем все равспознанные тачи с новыми
            recognizedMarkers = recognizedMarkers.Concat(newRecognizedTangibles
                    .ToDictionary(x => x.Id, x => x))
                .ToDictionary(x => x.Key, x => x.Value);
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
		
        //обновляем положение точек маркера
        private void UpdateRecognizedMarkers(List<TouchPoint> markerTouches)
        {
            // Added -> Updated
            // Update triangle position
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

        public void RemoveLostMarkers()
        {
            var removedMarkersIds = recognizedMarkers.Values.Where(marker => 
                    marker.Type == RecognizedTangibleMarker.ActionType.Removed)
                .Select(marker => marker.Id);
            foreach (var removedId in removedMarkersIds)
            {
                recognizedMarkers.Remove(removedId);
            }
        }
    }
}