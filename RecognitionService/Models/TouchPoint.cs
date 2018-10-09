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

        public ActionType type;
        public int id;
        public Vector2 Position;
        public Vector2 Acceleration;
    }
}
