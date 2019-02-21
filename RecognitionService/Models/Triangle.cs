using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace RecognitionService.Models
{
    public class Triangle : IEquatable<Triangle>
    {
        public const double defaultPrecision = 1e-3;

        public Vector2 posA { get; set; }
        public Vector2 posB { get; set; }
        public Vector2 posC { get; set; }

        private readonly List<Segment> sides;

        [JsonIgnore]
        public List<Segment> SortedSides
        {
            get
            {
                // In order to have updated length for each segment
                var sortedSides = new List<Segment>()
                {
                    new Segment(posA, posB),
                    new Segment(posB, posC),
                    new Segment(posC, posA)
                };
                sortedSides.Sort((v1, v2) => v1.Length >= v2.Length ? 1 : -1);
                return sortedSides;
            }
        }

        [JsonIgnore]
        public Segment ShortSide
        {
            get { return SortedSides[0]; }
        }

        [JsonIgnore]
        public Segment MiddleSide
        {
            get { return SortedSides[1]; }
        }

        [JsonIgnore]
        public Segment LargeSide
        {
            get { return SortedSides[2]; }
        }

        /* result = vector multiplication of short and large sides in coordinates
           define where point relative to vector
           result>0 => clockwise; result<0 => counter clockwise */
        public bool IsClockwiseRotated
        {
            get
            {
                var dotProduct =
                    (LargeSide.Origin.X - ShortSide.Origin.X) * (MiddleSide.Origin.Y - ShortSide.Origin.Y) - 
                    (LargeSide.Origin.Y - ShortSide.Origin.Y) * (MiddleSide.Origin.X - ShortSide.Origin.X);
                return dotProduct > 0;
            }
        }

        public Triangle(Vector2 posA, Vector2 posB, Vector2 posC)
        {
            this.posA = posA;
            this.posB = posB;
            this.posC = posC;

            this.sides = new List<Segment>()
            {
                new Segment(posA, posB),
                new Segment(posB, posC),
                new Segment(posC, posA)
            };
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
            return Equals(other, defaultPrecision);
        }

        public bool Equals(Triangle other, double precision = defaultPrecision)
        {
            var areEqual = ShortSide.EqualSegmentExistInList(other.SortedSides, precision) &&
                MiddleSide.EqualSegmentExistInList(other.SortedSides, precision) &&
                LargeSide.EqualSegmentExistInList(other.SortedSides, precision);

            return areEqual;
        }

        public float SimiliarityWith(Triangle other)
        {
            float meanError = (LargeSide.CompareWith(other.LargeSide) +
                               MiddleSide.CompareWith(other.MiddleSide) +
                               ShortSide.CompareWith(other.ShortSide)) / 3;
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
