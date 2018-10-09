using System;
using System.Numerics;
using System.Collections.Generic;

namespace RecognitionService.Models
{
    public struct RegistredTangibleMarker
    {
        public int Id;
        public Triangle triangle;
        public float initialAngle;
        public float angleToCenter;

        public List<Segment> Sides
        {
            get { return triangle.sides; }
        }

        public RegistredTangibleMarker(int id, Triangle triangle, float initialAngle, float angleToCenter)
        {
            this.Id = id;
            this.triangle = triangle;
            this.initialAngle = initialAngle;
            this.angleToCenter = angleToCenter;
        }
    }

    public struct RecognizedTangibleMarker
    {
        public int Id;
        public Triangle triangle;
        public float rotationAngle;
        public Vector2 center;

        public List<Segment> Sides
        {
            get { return triangle.sides; }
        }

        public RecognizedTangibleMarker(int id, Triangle triangle, float rotationAngle, Vector2 center)
        {
            this.Id = id;
            this.triangle = triangle;
            this.rotationAngle = rotationAngle;
            this.center = center;
        }
    }
}
