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
        private Dictionary<int, Dictionary<int, TouchPoint>> markerTouches = new Dictionary<int, Dictionary<int, TouchPoint>>();
        private HashSet<TouchPoint> touches = new HashSet<TouchPoint>();
        private List<RegistredTangibleMarker> _registredMarkers = new List<RegistredTangibleMarker>();

        public List<TouchPoint> LostTouches
        {
            get
            {
                //точки которые пропадут
                return touches.Where(t => t.Type == TouchPoint.ActionType.Up).ToList();
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
                var markerTouchesIds = markerTouches.Values.Select(dict => dict.Values.ToList())
                    .SelectMany(tp => tp).ToList().Select(t => t.Id);
                return touches.Where(t => markerTouchesIds.Contains(t.Id)).ToList();
            }
        }

        public List<TouchPoint> ValidTouches
        {
            get
            {
                //точки которые не входят в зареганные
                var markerTouchIds = MarkerTouches.Select(touch => touch.Id);
                return touches.Where(touch => !markerTouchIds.Contains(touch.Id)).ToList();
            }
        }

        public List<RegistredTangibleMarker> PassiveMarkers
        {
            get
            {
                return _registredMarkers.Where(reg => !markerTouches.ContainsKey(reg.Id)).ToList();
            }
        }
        /*
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
        */
        public HashSet<int> LostMarkersIds
        {
            get
            {
                HashSet<int> lostMarkersIds = new HashSet<int>();

                foreach (var lostTouch in LostTouches)
                {
                    foreach (var touchesTripletDict in markerTouches.Values)
                    {
                        if (touchesTripletDict.ContainsKey(lostTouch.Id))
                        {
                            lostMarkersIds.Add(lostTouch.Id);
                        }
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

        public MarkerActivityController() { }

        public void ProcessTouchPointFrame(TouchPointFrame frame, List<RegistredTangibleMarker> registredMarkers)
        {
            touches = new HashSet<TouchPoint>(frame.touches);
            _registredMarkers = registredMarkers;
            //в первую очередь помечаем удаленные маркеры как Removed и удаляем их тачи из словаря
            MarkLostMarkers();
            //обновляем словарь с тачами
            UpdateMarkerTouches();
            //обновляем положение распознанных маркеров  на основе словаря, Added переведем в updated только на сл шане у них обновляется только центр
            UpdateRecognizedMarkers(); // все Addded которые остались с предыдущего шага пометим Updated
        }

        private void MarkLostMarkers()
        {
            foreach (var markerId in LostMarkersIds)
            {
                markerTouches.Remove(markerId);
                _recognizedMarkers[markerId].Type = RecognizedTangibleMarker.ActionType.Removed;
            }
        }

        public void UpdateMarkerTouches()
        {
            foreach (var touch in MarkerTouches)
            {
                foreach (var touchTripletDict in markerTouches.Values)
                {
                    if (touchTripletDict.ContainsKey(touch.Id))
                    {
                        touchTripletDict[touch.Id] = touch;
                    }
                }
            }
        }

        public void AddRecognizedMarkers(List<RecognizedTangibleMarker> newRecognizedTangibles)
        {
            //добавляем новые тачи в словарь
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
            markerTouches[newMarker.Id] = new Dictionary<int, TouchPoint>()
            {
                {newMarker.TouchPointMap[0].id, newMarker.TouchPointMap[0]},
                {newMarker.TouchPointMap[1].id, newMarker.TouchPointMap[1]},
                {newMarker.TouchPointMap[2].id, newMarker.TouchPointMap[2]}
            };
        }

        //обновляем положение точек маркера
        private void UpdateRecognizedMarkers()
        {
            // Added -> Updated
            // Update triangle position
            Dictionary<int, TouchPoint> markerTouchesDictionary = MarkerTouches.ToDictionary(o => o.Id);
            foreach (var markerId in markerTouches.Keys)
            {
                _recognizedMarkers[markerId].UpdateVertexes(markerTouches[markerId]);
                //update sides
                _recognizedMarkers[markerId].Type = RecognizedTangibleMarker.ActionType.Updated;
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