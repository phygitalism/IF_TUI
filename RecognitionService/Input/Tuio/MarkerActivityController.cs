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
        private Dictionary<int, HashSet<int>> markerTouches = new Dictionary<int, HashSet<int>>();
        private HashSet<TouchPoint> touches = new HashSet<TouchPoint>();
        private List<RegistredTangibleMarker> _registredMarkers = new List<RegistredTangibleMarker>();
        
        public List<TouchPoint> LostTouches
        {
            get
            {
                //точки которые пропадут
                return touches.Where(t => t.type == TouchPoint.ActionType.Up).ToList();
            }
        }

        public List<TouchPoint> MarkerTouches
        {
           /* get
            {
                //точки которые входят в зареганные
                return markerTouches.Values
                    .Select(touchePoints => touchePoints.ToList())
                    .SelectMany(touchPoint => touchPoint).ToList();
            }*/
           //нужны тачи с рамки они новые
            get
            {
                //точки которые входят в зареганные
                var markerTouchesIds = markerTouches.Values.Select(set => set.ToList())
                    .SelectMany(id => id).ToList();
                return touches.Where(t => markerTouchesIds.Contains(t.id)).ToList(); 
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
                return _registredMarkers.Where(reg => !markerTouches.ContainsKey(reg.Id)).ToList();
            }
        }

        public Dictionary<int, int> ReverseMarkerTouches
        {
            get
            {
                Dictionary<int, int> reverseMarkerTouches = new Dictionary<int, int>();
                foreach (var markerId in markerTouches.Keys)
                {
                    foreach (var touchPointId in markerTouches[markerId])
                    {
                        reverseMarkerTouches[touchPointId] = markerId;
                    }
                }
                return reverseMarkerTouches;
            }
            
        }

        public HashSet<int> LostMarkersIds
        {
            get
            {
                HashSet<int> lostMarkersIds = new HashSet<int>();
                var reverseMarkerTouches = ReverseMarkerTouches;
                foreach (var lostTouch in LostTouches)
                {
                    if (reverseMarkerTouches.ContainsKey(lostTouch.id))
                    {
                        lostMarkersIds.Add(reverseMarkerTouches[lostTouch.id]);
                    }
                }
                return lostMarkersIds;
            }
        }
        
        public List<RecognizedTangibleMarker> ActiveMarkers
        {
            get
            {    
               //return _registredTangibles.Where(reg => !markerTouches.ContainsKey(reg.Id)).ToList();
               return _recognizedMarkers.Values.ToList();
            }   
        }
        
        public MarkerActivityController(){ }
        
        public void ProcessTouchPointFrame(TouchPointFrame frame, List<RegistredTangibleMarker> registredMarkers)
        {
            touches = new HashSet<TouchPoint>(frame.touches);
            _registredMarkers = registredMarkers;
            // update and remove
            //обновляем положение распознанных маркеров, для Added обновляется только центр
            UpdateRecognizedMarkers(); // все Addded пометим Updated
            //помечаем удаленные маркеры как Removed и удаляем их тачи из словаря
            MarkLostMarkers();
        }

        private void MarkLostMarkers()
        {
            foreach (var markerId in LostMarkersIds)
            {
                markerTouches.Remove(markerId);
                _recognizedMarkers[markerId].Type = RecognizedTangibleMarker.ActionType.Removed;
            }
        }


        public void AddRecognizedMarkers(List<RecognizedTangibleMarker> newRecognizedTangibles)
        {
            //
            foreach (var marker in newRecognizedTangibles)
            {
                AddMarkerTouches(marker);
            }
            //соединяем все равспознанные тачи с новыми
            _recognizedMarkers = _recognizedMarkers.Concat(newRecognizedTangibles
                    .ToDictionary(marker => marker.Id, marker => marker))
                .ToDictionary(x => x.Key, x => x.Value);
        }

        private void AddMarkerTouches(RecognizedTangibleMarker newMarker)
        {
            markerTouches[newMarker.Id] = new HashSet<int>()
            {
                newMarker.verteciesIds[0],
                newMarker.verteciesIds[1],
                newMarker.verteciesIds[2]
            };
        }

        //обновляем положение точек маркера
        private void UpdateRecognizedMarkers()
        {
            // Added -> Updated
            // Update triangle position
            
            //потом подумаю как переписать нормально - проблема что обновленные тачи хранятся по отдельности не можем сразу сетом их брать
            //TODO update position by updated markerTouches 
            Dictionary<int, TouchPoint> markerTouchesDictionary = MarkerTouches.ToDictionary(o => o.id);
            var reverseMarkerTouches = ReverseMarkerTouches;
            foreach (var touchId in reverseMarkerTouches.Keys)
            {
                int currentMarkerId = reverseMarkerTouches[touchId];
                TouchPoint currentTouch = markerTouchesDictionary[touchId];
                _recognizedMarkers[currentMarkerId].UpdatePosition(currentTouch);
                _recognizedMarkers[currentMarkerId].UpdateSides();
                _recognizedMarkers[currentMarkerId].Type = RecognizedTangibleMarker.ActionType.Updated;
            }
        }

        public void RemoveLostMarkers()
        {
            var removedMarkersIds = _recognizedMarkers.Values.Where(marker => 
                    marker.Type == RecognizedTangibleMarker.ActionType.Removed)
                .Select(marker => marker.Id);
            foreach (var removedId in removedMarkersIds)
            {
                _recognizedMarkers.Remove(removedId);
            }
        }
    }
}