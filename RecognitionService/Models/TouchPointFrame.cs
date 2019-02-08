using System;
using System.Collections.Generic;
using System.Linq;

namespace RecognitionService.Models
{
    public struct TouchPointFrame
    {
        public int Id;
        public int timestamp;
        public List<TouchPoint> touches;


        public int Count
        {
            get { return touches.Count; }
        }

        public TouchPointFrame(int id, int timestamp, List<TouchPoint> touches)
        {
            this.Id = id;
            this.timestamp = timestamp;
            this.touches = touches;
        }

        public List<TouchPoint> ExtractValidTouches(Dictionary<int, int> markerTouchesIdToMarkersId)
        {
            //точки которые не входят в зареганные 
            var validTouches = touches.Where(t => !markerTouchesIdToMarkersId.Keys.Contains(t.id)).ToList(); 
            return validTouches;
        }
        
        public List<TouchPoint> ExtractMarkerTouches(Dictionary<int, int> markerTouchesIdToMarkersId)
        {
            //точки которые входят в зареганные 
            var markerTouches = touches.Where(t => markerTouchesIdToMarkersId.Keys.Contains(t.id)).ToList(); 
            return markerTouches;
        }
    }

}
