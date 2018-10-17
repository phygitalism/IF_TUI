using System;
using System.Linq;
using System.Numerics;
using System.Collections.Generic;

using Newtonsoft.Json;

namespace RecognitionService.Models
{
    public struct Triangle : IEquatable<Triangle>
    {
        public class NonExistentTriangle : Exception
        {
            public NonExistentTriangle(string message) : base(message) { }
        }

        public Vector2 posA;
        public Vector2 posB;
        public Vector2 posC;

        public List<Segment> sides;

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
            this.sides.Sort((v1, v2) => v1.length >= v2.length ? 1 : -1);
        }

        public Triangle(Segment sideA, Segment sideB, Segment sideC)
        {
            var sides = new List<Segment>() { sideA, sideB, sideC };

            var points = sides.SelectMany(side => new Vector2[] { side.origin, side.destination }).ToList();

            var vertecies = RemoveDuplicates(points); // TODO - compare points with precision
            if (vertecies.Count != 3)
            {
                throw new Triangle.NonExistentTriangle("Unable to determine vertices");
            }

            this.posA = vertecies[0];
            this.posB = vertecies[1];
            this.posC = vertecies[2];

            this.sides = new List<Segment>()
            {
                new Segment(this.posA, this.posB),
                new Segment(posB, posC),
                new Segment(posC, posA)
            };
            this.sides.Sort((v1, v2) => v1.length >= v2.length ? 1 : -1);
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
