using System;
using System.Linq;
using System.Collections.Generic;

namespace RecognitionService.Models
{
    public struct TouchPointFrame
    {
        public readonly int Id;
        public readonly int Timestamp;
        public readonly List<TouchPoint> Touches;

        public Dictionary<int, TouchPoint> Lookup;

        public int Count
        {
            get { return Touches.Count; }
        }

        public List<TouchPoint> NewTouches
        {
            get { return Touches.Where(t => t.Type == TouchPoint.ActionType.Down).ToList(); }
        }

        public List<TouchPoint> ActiveTouches
        {
            get { return Touches.Where(t => t.Type == TouchPoint.ActionType.Move).ToList(); }
        }

        public List<TouchPoint> LostTouches
        {
            get { return Touches.Where(t => t.Type == TouchPoint.ActionType.Up).ToList(); }
        }

        public TouchPointFrame(int id, int timestamp, List<TouchPoint> touches)
        {
            this.Id = id;
            this.Timestamp = timestamp;
            this.Touches = touches;
            this.Lookup = touches.ToDictionary(touch => touch.Id);
        }
    }
}
