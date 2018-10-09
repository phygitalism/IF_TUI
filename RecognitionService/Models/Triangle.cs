using System;
using System.Numerics;
using System.Collections.Generic;

namespace RecognitionService.Models
{
    public struct Triangle
    {
        public Vector2 posA;
        public Vector2 posB;
        public Vector2 posC;

        public List<Segment> sides;

        public Segment ShortSide
        {
            get { return sides[0]; }
        }

        public Segment MiddleSide
        {
            get { return sides[1]; }
        }

        public Segment LargeSide
        {
            get { return sides[2]; }
        }

        public Triangle(Vector2 posA, Vector2 posB, Vector2 posC)
        {
            this.posA = posA;
            this.posB = posB;
            this.posC = posC;

            sides = new List<Segment>()
            {
                new Segment(posA, posB),
                new Segment(posB, posC),
                new Segment(posC, posA)
            };
            sides.Sort((v1, v2) => v2.length >= v1.length ? 1 : -1);
        }
    }
}
