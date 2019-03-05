using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;

using RecognitionService.Models;

namespace RecognitionService.Input.Tuio
{
    public class MarkerActivityController
    {
        private List<RegistredTangibleMarker> _registredMarkers = new List<RegistredTangibleMarker>();

        // recognizedMarkers - активные маркеры
        private Dictionary<int, RecognizedTangibleMarker> _recognizedMarkers = new Dictionary<int, RecognizedTangibleMarker>();
        // соотвествие тачей распознаным маркерам
        private Dictionary<int, Dictionary<int, TouchPoint>> _markerTouches = new Dictionary<int, Dictionary<int, TouchPoint>>();
        
        private TouchPointFrame _frame;

        // тачи которые соотвествуют распознаным маркерам
        public List<TouchPoint> MarkerTouches
        {
            get
            {
                return _markerTouches.Values
                    .Select(dict => dict.Values.ToList())
                    .SelectMany(tp => tp).ToList();
            }
        }

        // тачи которые не соотвествуют распознаным маркерам
        public List<TouchPoint> PassiveTouches
        {
            get
            {   
                var markerTouchIds = MarkerTouches.Select(touch => touch.Id);
                return _frame.Touches.Where(touch => !markerTouchIds.Contains(touch.Id)).ToList();
            }
        }

        public List<RegistredTangibleMarker> PassiveMarkers
        {
            get { return _registredMarkers.Where(reg => !_recognizedMarkers.ContainsKey(reg.Id)).ToList(); }
        }

        public List<RegistredTangibleMarker> UnstableMarkers
        {
            get
            {
                var unstableMarkersIds = _recognizedMarkers
                    .Where(rec => rec.Value.Type == RecognizedTangibleMarker.ActionType.Unstable)
                    .Select(marker => marker.Key);
                return _registredMarkers.Where(reg => unstableMarkersIds.Contains(reg.Id)).ToList();
            }
        }

        public List<RecognizedTangibleMarker> LostMarkers
        {
            get
            {
                return _recognizedMarkers.Values
                    .Where(marker => marker.Type == RecognizedTangibleMarker.ActionType.Removed)
                    .ToList();
            }
        }

        public List<RecognizedTangibleMarker> ActiveMarkers
        {
            get { return _recognizedMarkers.Values.ToList(); }
        }

        public MarkerActivityController() { }

        public void ProcessTouchPointFrame(TouchPointFrame frame, List<RegistredTangibleMarker> registredMarkers)
        {
            _frame = frame;
            _registredMarkers = registredMarkers;
            RemoveUnstableTouchesFromMarkerTouches();
            UpdateMarkersTouches();
        }

        public void UpdateMarkersTouches()
        {
            foreach (var markerId in _recognizedMarkers.Keys)
            {
                UpdateMarkerTouches(markerId); 
            }
        }

        private void UpdateMarkerTouches(int markerId)
        {
            var touchesForMarker = StableTouchesForMarker(markerId);
            try
            {
                var touchesForMarkerIds = new List<int>(touchesForMarker.Keys);
                foreach (var touchId in touchesForMarkerIds)
                {
                    var touchFromFrame = _frame.Lookup[touchId];
                    touchesForMarker[touchId] = touchFromFrame;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("UpdateMarkerTouches: Can't update touch for marker");
            }

            // распознаный маркер сам обновляет свое состояние и обновляет вершины своего треугольника
            // Addded с предыдущего шага перейдет в Updated
            // если тач, входящий в маркер, пропал, то в Removed
            _recognizedMarkers[markerId].UpdateVertexes(touchesForMarker.Values.ToList());
        }

        private Dictionary<int, TouchPoint> StableTouchesForMarker(int markerId)
        {
            if (_recognizedMarkers[markerId].Type != RecognizedTangibleMarker.ActionType.Unstable)
            {
                return _markerTouches[markerId];
            }
            
            return _recognizedMarkers[markerId].ActiveTouchPoints.
                    Where(elem => elem.Value.Type != TouchPoint.ActionType.Up).ToDictionary(x => x.Key, x=> x.Value);
        }

        public void AddRecognizedMarkers(List<RecognizedTangibleMarker> newRecognizedTangibles)
        {
            foreach (var marker in newRecognizedTangibles)
            {
                if (marker.Type == RecognizedTangibleMarker.ActionType.Updated || marker.Type == RecognizedTangibleMarker.ActionType.Removed)
                {
                    continue;
                }
                if (_recognizedMarkers.ContainsKey(marker.Id))
                {
                    marker.Type = RecognizedTangibleMarker.ActionType.Updated;
                }
                else
                {
                    // если у нового распознанного маркера есть такие же айди как у оставшихся ножек нестабильного маркера 
                    // и он не нестабильный значит новый маркер самозванец и мародер захватил оторванные конечности нестабильного маркера себе 
                    var stableTouchIds = _recognizedMarkers[marker.Id].ActiveTouchPoints
                        .Where(elem => elem.Value.Type != TouchPoint.ActionType.Up).Select(elem => elem.Key);
                    if (stableTouchIds.Any(id => marker.ActiveTouchPoints.Keys.Contains(id)))
                    {
                        continue;
                    }
                }

                AddMarkerTouches(marker);
                _recognizedMarkers[marker.Id] = marker;
            }
        }

        private void AddMarkerTouches(RecognizedTangibleMarker newMarker)
        {
            var touchesForMarker = new Dictionary<int, TouchPoint>();

            foreach (var touchPointId in newMarker.TouchPointMap.Keys)
            {
                touchesForMarker[touchPointId] = _frame.Lookup[touchPointId];
            }
            _markerTouches[newMarker.Id] = touchesForMarker;
        }

        private void RemoveUnstableTouchesFromMarkerTouches()
        {
            var unstableMarkerIds = UnstableMarkers.Select(marker => marker.Id);
            foreach (var unstableMarkerId in unstableMarkerIds)
            {
                _markerTouches.Remove(unstableMarkerId);
            }
        }

        public void RemoveLostMarkers()
        {
            var lostMarkerIds = LostMarkers.Select(marker => marker.Id);
            foreach (var lostMarkerId in lostMarkerIds)
            {
                _recognizedMarkers.Remove(lostMarkerId);
                _markerTouches.Remove(lostMarkerId);
            }
        }
    }
}