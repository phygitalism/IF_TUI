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

        public int id;
        public Vector2 Position;
        public Vector2 Acceleration;
        public ActionType type;

        public TouchPoint(int id, Vector2 position, Vector2 acceleration, ActionType type)
        {
            this.id = id;
            this.Position = position;
            this.Acceleration = acceleration;
            this.type = type;
        }
    }
}
