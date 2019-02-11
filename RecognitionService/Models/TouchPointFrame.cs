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
    }

}
