using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

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
        
        public float CalculateAngleBetweenY()
        {
            Vector2 segmentFromZero = destination - origin;
            float radians = (float) Math.Atan2(segmentFromZero.Y, segmentFromZero.X);
            return radians;
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
            var equalSegments = segments.Where(segment => Math.Abs(segment.length - source.length) <= precision).ToList();
            return equalSegments;
        }

        public static float CompareSegment(this Segment source, Segment other)
        {
            return Math.Abs(source.length - other.length);
        }
    }
}
