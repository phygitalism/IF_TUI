
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using RecognitionService.Models;

namespace RecognitionService
{
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
