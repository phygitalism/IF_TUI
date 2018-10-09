using System;
using System.Numerics;

namespace RecognitionService.Models
{
    public struct Segment
    {
        public Vector2 origin;
        public Vector2 destination;

        public float length;

        public Segment(Vector2 origin, Vector2 destination)
        {
            this.origin = origin;
            this.destination = destination;
            this.length = Vector2.Distance(origin, destination);
        }
    }
}
