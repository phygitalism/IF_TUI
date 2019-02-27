using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

namespace RecognitionService.Models
{
    public struct Segment
    {
        public Vector2 Origin { get; set; }
        public Vector2 Destination { get; set; }

        public float Length
        {
            get { return Vector2.Distance(Origin, Destination); }
        }

        public Segment(Vector2 origin, Vector2 destination)
        {
            this.Origin = origin;
            this.Destination = destination;
        }
        
        public float CalculateAngleBetweenY()
        {
            Vector2 segmentFromZero = Destination - Origin;
            float radians = (float) Math.Atan2(segmentFromZero.Y, segmentFromZero.X);
            return radians;
        }
        
        public bool isPerpendicularToAxes()
        {      
            return isPerpendicularToX() || isPerpendicularToY();
        }

        public bool isPerpendicularToX()
        {
            var xDelta = Math.Abs(Destination.X - Origin.X);
            return xDelta < 1e-3;
        }

        public bool isPerpendicularToY()
        {
            var yDelta = Math.Abs(Destination.Y - Origin.Y);
            return yDelta < 1e-3;
        }
		
        private float radToDeg(float rad)
        {
            return (float)((rad * 180)/Math.PI);
        }
    }

    public static class SegmentExtensions
    {
        public static bool EqualSegmentExistInList(this Segment source, List<Segment> segments, double precision = 1e-3)
        {
            return EqualSegmentsInList(source, segments, precision).Count > 0;
        }

        public static List<Segment> EqualSegmentsInList(this Segment source, List<Segment> segments, double precision = 1e-3)
        {
            var equalSegments = segments.Where(segment => Math.Abs(segment.Length - source.Length) <= precision).ToList();
            return equalSegments;
        }

        public static float CompareWith(this Segment source, Segment other)
        {
            return Math.Abs(source.Length - other.Length);
        }
    }
}
