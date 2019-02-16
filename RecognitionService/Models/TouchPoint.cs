using System;
using System.Numerics;

namespace RecognitionService.Models
{
    public struct TouchPoint
    {
        public enum ActionType
        {
            Down = 0,
            Move = 1,
            Up = 2
        }

        public int Id { get; private set; }
        public ActionType Type { get; private set; }

        public Vector2 Position { get; set; }
        public Vector2 Acceleration { get; set; }
        
        public TouchPoint(int id, Vector2 position, Vector2 acceleration, ActionType type)
        {
            this.Id = id;
            this.Position = position;
            this.Acceleration = acceleration;
            this.Type = type;
        }

        public TouchPoint ToRelativeCoordinates(float width, float height)
        {
            var relativePosition = new Vector2(Position.X / width, Position.Y / height);
            var relativeAcceleration = new Vector2(Acceleration.X / width, Acceleration.Y / height);
            return new TouchPoint(Id, relativePosition, relativeAcceleration, Type);
        }
    }
}
