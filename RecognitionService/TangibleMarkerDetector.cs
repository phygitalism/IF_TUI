
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

namespace RecognitionService
{
    public enum TouchPointType
    {
        Down = 0,
        Move = 1,
        Up = 2
    }
    public struct TouchPoint
    {
        public TouchPointType type;
        public int id;
        public Vector2 Position;
        public Vector2 Acceleration;
    }

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

    public struct Triangle
    {
        public Vector2 posA;
        public Vector2 posB;
        public Vector2 posC;

        public List<Segment> segments;

        public Segment Short
        {
            get { return segments[0]; }
        }

        public Segment Middle
        {
            get { return segments[1]; }
        }

        public Segment Large
        {
            get { return segments[2]; }
        }

        public Triangle(Vector2 posA, Vector2 posB, Vector2 posC)
        {
            this.posA = posA;
            this.posB = posB;
            this.posC = posC;

            segments = new List<Segment>()
            {
                new Segment(posA, posB),
                new Segment(posB, posC),
                new Segment(posC, posA)
            };
            segments.Sort((v1, v2) => v2.length >= v1.length ? 1 : -1);
        }
    }

    public struct TangibleMarker
    {
        public Triangle triangle;
        public float rotationAngle;
        public float initialAngle;
        public float angleToCenter;

        public List<Segment> Sides
        {
            get { return triangle.segments; }
        }
    }

    class TangibleMarkerDetector
    {
        private const float physicalMarkerDiameter = 9;
        private const double precision = 8e-3;

        private List<TangibleMarker> _knownMarkers;

        public TangibleMarkerDetector(List<TangibleMarker> knownMarkers)
        {
            this._knownMarkers = knownMarkers;
        }

        public void DetectTangibleMarkers(List<TouchPoint> frame)
        {
            var allPossibleTriangles = DistinguishTriangles(frame);
        }

        private List<Triangle> DistinguishTriangles(List<TouchPoint> frame)
        {
            var segments = ConnectAllPointsToEachOthers(frame);
            var allKnownSegments = _knownMarkers.SelectMany(marker => marker.Sides).ToList();
            var markerSegments = segments
                .Where(segment => segment.length <= physicalMarkerDiameter)
                .Where(segment => EqualSegmentExistInList(segment, allKnownSegments, precision))
                .ToList();

            // try to constuct triangles
            foreach (var tangibleMarker in _knownMarkers)
            {
                foreach (var side in tangibleMarker.Sides)
                {

                }
            }

            return new List<Triangle>();
        }

        private bool EqualSegmentExistInList(Segment source, List<Segment> segments, double precision)
        {
            return EqualSegmentsInList(source, segments, precision).Count > 0;
        }

        private List<Segment> EqualSegmentsInList(Segment source, List<Segment> segments, double precision = 1e-3)
        {
            var equalSegments = segments.Where(segment => Math.Abs(segment.length - source.length) <= precision).ToList();
            return equalSegments;
        }

        private List<Segment> ConnectAllPointsToEachOthers(List<TouchPoint> frame)
        {
            var enumeratedPoints = frame.Select((tp, i) => (i, tp.Position)).ToList();
            var amount = enumeratedPoints.Count;
            var allPossibleSegments = new List<Segment>();

            foreach ((var index, var point) in enumeratedPoints)
            {
                foreach ((var index2, var point2) in enumeratedPoints.Slice(index + 1, amount))
                {
                    allPossibleSegments.Add(new Segment(point, point2));
                }
            }

            return allPossibleSegments;
        }
    }
}