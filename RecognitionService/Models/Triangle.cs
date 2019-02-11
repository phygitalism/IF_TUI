using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace RecognitionService.Models
{
    public struct Triangle : IEquatable<Triangle>
    {
        public Vector2 posA;
        public Vector2 posB;
        public Vector2 posC;
        
        [JsonIgnore]
        public TouchPoint touchA  { get; set; }
        public TouchPoint touchB { get; set; }
        public TouchPoint touchC { get; set; }

        public List<Segment> sides;
        public Dictionary<int, TouchPoint> vertex;

		[JsonIgnore]
        public Segment ShortSide
        {
            get { return sides[0]; }
        }

        [JsonIgnore]
        public Segment MiddleSide
        {
            get { return sides[1]; }
        }

        [JsonIgnore]
        public Segment LargeSide
        {
            get { return sides[2]; }
        }

        /* result = vector multiplication of short and large sides in coordinates
           define where point relative to vector
           result>0 => clockwise; result<0 => counter clockwise */
        public bool ClockwiseRotation
        {
            get { return (((LargeSide.origin.Position.X - ShortSide.origin.Position.X) * (MiddleSide.origin.Position.Y - ShortSide.origin.Position.Y) - 
                           (LargeSide.origin.Position.Y - ShortSide.origin.Position.Y) * (MiddleSide.origin.Position.X - ShortSide.origin.Position.X)) > 0);
            }
        }

        public Triangle(TouchPoint touchA, TouchPoint touchB, TouchPoint touchC)
        {
            this.touchA = touchA;
            this.touchB = touchB;
            this.touchC = touchC;

            this.posA = touchA.Position;
            this.posB = touchB.Position;
            this.posC = touchC.Position;

            this.sides = new List<Segment>()
            {
                new Segment(touchA, touchB),
                new Segment(touchB, touchC),
                new Segment(touchC, touchA)
            };
            this.vertex = new Dictionary<int, TouchPoint>()
            {
                {touchA.id, touchA},
                {touchB.id, touchB},
                {touchC.id, touchC}
            };
            this.sides.Sort((v1, v2) => v1.length >= v2.length ? 1 : -1);
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Triangle))
            {
                return false;
            }

            var other = (Triangle)obj;
            return this.Equals(other);
        }

		public bool Equals(Triangle other)
		{
			return Equals(other, 1e-3);
		}

		public bool Equals(Triangle other, double precision = 1e-3)
        {
            var areEqual = ShortSide.EqualSegmentExistInList(other.sides, precision) &&
                MiddleSide.EqualSegmentExistInList(other.sides, precision) &&
                LargeSide.EqualSegmentExistInList(other.sides, precision);

            return areEqual;
        }

        public float SimiliarityWith(Triangle other)
        {
            float meanError = (LargeSide.CompareWith(other.LargeSide) +
                               MiddleSide.CompareWith(other.MiddleSide) +
                               ShortSide.CompareWith(other.ShortSide))/3;
            return meanError;
        }
        
        /*
        public static bool TryBuildFromSegments(Segment sideA, Segment sideB, Segment sideC, out Triangle? triangle)
        {
            triangle = null;
            var sides = new List<Segment>() { sideA, sideB, sideC };

            var points = sides.SelectMany(side => new Vector2[] { side.origin, side.destination }).ToList();

            var vertecies = RemoveDuplicates(points); // TODO - compare points with precision
            if (vertecies.Count != 3)
            {
                return false;
            }

            triangle = new Triangle(vertecies[0], vertecies[1], vertecies[2]);
            return true;
        }

        private static List<Vector2> RemoveDuplicates(List<Vector2> originalList)
        {
            HashSet<Vector2> set = new HashSet<Vector2>();

            foreach (var obj in originalList)
            {
                if (!set.Contains(obj))
                {
                    set.Add(obj);
                }
            }
            return set.ToList();
        }
*/

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = ShortSide.GetHashCode();
                hashCode = (hashCode * 397) ^ MiddleSide.GetHashCode();
                hashCode = (hashCode * 397) ^ LargeSide.GetHashCode();
                return hashCode;
            }
        }
    }
}
