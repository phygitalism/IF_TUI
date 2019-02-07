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

        public List<TouchPoint> ExtractValidTouches(Dictionary<int, int> markerTouches)
        {
            //var validTouches = touches.Except(markerTouches);
            //точки которые не входят в зареганные 
            var validTouches = touches.Where(t => !markerTouches.Keys.Any(mt => mt == t.id)).ToList();
            return validTouches;
        }
    }

}
