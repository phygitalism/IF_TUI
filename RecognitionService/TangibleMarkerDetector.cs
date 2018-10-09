
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
        private const double tolerance = 8e-3;

        private List<TangibleMarker> _knownMarkers;

        public TangibleMarkerDetector(List<TangibleMarker> knownMarkers)
        {
            this._knownMarkers = knownMarkers;
        }

        public void DetectTangibleMarkers(List<TouchPoint> frame)
        {
            var allPossibleTriangles = DistinguishTriangles(frame);

            // TODO - determine Ids for triangles
        }

        private List<Triangle> DistinguishTriangles(List<TouchPoint> frame)
        {
            var segments = ConnectAllPointsToEachOthers(frame);
            var allKnownSegments = _knownMarkers.SelectMany(marker => marker.Sides).ToList();
            var markerSegments = segments
                .Where(segment => segment.length <= physicalMarkerDiameter)
                .Where(segment => segment.EqualSegmentExistInList(allKnownSegments, tolerance))
                .ToList();

            var distinguishedTriangles = ConstructTriangles(markerSegments);

            return distinguishedTriangles;
        }

        private List<Triangle> ConstructTriangles(List<Segment> segments)
        {
            var constructedTriangles = new List<Triangle>();

            var combinationsOfSegments = segments.GetCombinationsWithoutRepetition(3);
            foreach (var combinationOfSegments in combinationsOfSegments)
            {
                var sides = combinationOfSegments.ToList();
                if (sides.Count != 3)
                {
                    continue;
                }

                Triangle triangle;
                try
                {
                    triangle = new Triangle(sides[0], sides[1], sides[2]);
                    constructedTriangles.Add(triangle);
                }
                catch (Triangle.NonExistentTriangle ex)
                {
                    Console.WriteLine(ex);
                }
            }

            return constructedTriangles;
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
