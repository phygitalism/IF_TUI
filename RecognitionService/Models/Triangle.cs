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

        /* result = vector multiplication of short and large sides in coordinates
           define where point relative to vector
           result>0 => clockwise; result<0 => counter clockwise */
        public bool ClockwiseRotation
        {
            get { return (((LargeSide.origin.X - ShortSide.origin.X) * (MiddleSide.origin.Y - ShortSide.origin.Y) - 
                           (LargeSide.origin.Y - ShortSide.origin.Y) * (MiddleSide.origin.X - ShortSide.origin.X)) > 0);
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
