
using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;

using RecognitionService.Models;

namespace RecognitionService.Recognition
{
	class TangibleMarkerRecognizer : ITangibleMarkerRecognizer
	{
		private const float physicalMarkerDiameter = 120f;
		private const double tolerance = 5f;

		private List<RegistredTangibleMarker> _knownMarkers = new List<RegistredTangibleMarker>();

		public TangibleMarkerRecognizer() { }

		public List<RecognizedTangibleMarker> RecognizeTangibleMarkers(
			List<TouchPoint> frame, 
			List<RegistredTangibleMarker> knownMarkers
		)
		{
            if (frame.Count < 3)
            {
                return new List<RecognizedTangibleMarker>();
            }

			_knownMarkers = knownMarkers;
			var allPossibleTriangles = CreateTrianglesFromTouches(frame);
			var recognizedMarkers = new List<RecognizedTangibleMarker>();

			foreach (var triangle in allPossibleTriangles)
			{
				var knownTangibleMarker = FindTangibleMarkerForTriangle(triangle);
				if (knownTangibleMarker!=null)
				{
					var recognizedMarker = new RecognizedTangibleMarker(
						knownTangibleMarker.Id,
						triangle,
						knownTangibleMarker.initialAngle
					);

					recognizedMarkers.Add(recognizedMarker);
				}
			}
			return recognizedMarkers;
		}

		private List<Triangle> CreateTrianglesFromTouches(List<TouchPoint> frame)
		{
			var constructedTriangles = new List<Triangle>();

			var combinationsOfTouches = frame.GetCombinationsWithoutRepetition(3);
			foreach (var combinationOfTouches in combinationsOfTouches)
			{
				var touches = combinationOfTouches.ToList();
				if (touches.Count != 3)
				{
					continue;
				}
				Triangle triangle = new Triangle(touches[0], touches[1], touches[2]);
				if (triangle.LargeSide.length <= physicalMarkerDiameter)
				{
					constructedTriangles.Add(triangle);
				}
				else
				{
					//Console.WriteLine(ex);
				}
			}
			return constructedTriangles;
		}

		private RegistredTangibleMarker FindTangibleMarkerForTriangle(Triangle triangle)
		{
			List<(RegistredTangibleMarker, float)> pretenderMarkers = new List<(RegistredTangibleMarker, float)>();
			foreach (var tangibleMarker in _knownMarkers)
			{
				float sidesMeanError = triangle.SimiliarityWith(tangibleMarker.triangle);
				if (sidesMeanError < tolerance)
				{
					pretenderMarkers.Add((tangibleMarker, sidesMeanError));
				}
			}
			if (pretenderMarkers.Count > 0)
			{
				return ChooseMostSimilarMarker(pretenderMarkers);
			}
			return null;
		}
		/*
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

				if (Triangle.TryBuildFromSegments(sides[0], sides[1], sides[2], out var triangle) && triangle != null)
				{
					constructedTriangles.Add(triangle.Value);
				}
				else
				{
					//Console.WriteLine(ex);
				}
			}

			return constructedTriangles;
		}
		
		private List<Triangle> DistinguishTriangles(List<TouchPoint> frame)
		{
			var segments = ConnectAllPointsToEachOthers(frame);
			var allKnownSegments = _knownMarkers.SelectMany(marker => marker.Sides).ToList();
			var markerSegments = segments
				.Where(segment => segment.length <= physicalMarkerDiameter)
				//.Where(segment => segment.EqualSegmentExistInList(allKnownSegments, tolerance))
				.ToList();

			var distinguishedTriangles = ConstructTriangles(markerSegments);

			return distinguishedTriangles;
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
		*/

		// in case when several markers correspond to the same triangle
		private RegistredTangibleMarker ChooseMostSimilarMarker(List<(RegistredTangibleMarker, float)> pretenderMarkers)
		{
			pretenderMarkers.Sort((pair1, pair2) => pair1.Item2 >= pair2.Item2 ? 1 : -1);
			return pretenderMarkers[0].Item1;
		}
	}
}
