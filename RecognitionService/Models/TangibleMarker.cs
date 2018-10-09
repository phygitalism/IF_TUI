using System;
using System.Numerics;
using System.Collections.Generic;

namespace RecognitionService.Models
{
    public struct TangibleMarker
    {
        public Triangle triangle;
        public float rotationAngle;
        public float initialAngle;
        public float angleToCenter;

        public List<Segment> Sides
        {
            get { return triangle.sides; }
        }
    }
}
