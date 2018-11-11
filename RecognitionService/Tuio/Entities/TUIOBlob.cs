using System;
using OSCsharp.Data;
using TUIOsharp.Entities;

namespace RecognitionService.Tuio.Entities
{

    public class TUIOBlob : TuioBlob, ITUIOEntity
    {
        private const double precision = 1e-5;

        public OscMessage oscMessage { get; private set; }
        public bool isSendRequired { get; set; }

        public event EntityUpdatedHandler onUpdated;


        public TUIOBlob(int id) : this(id, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0) { }

        public TUIOBlob(int id, float x, float y, float angle, float width, float height, float area, float velocityX, float velocityY, float rotationVelocity, float acceleration, float rotationAcceleration)
            : base(id, x, y, angle, width, height, area, velocityX, velocityY, rotationVelocity, acceleration, rotationAcceleration)
        {

            // http://www.tuio.org/?specification - Profiles
            // /tuio/2Dblb set s x y a w h f X Y A m r
            oscMessage = new OscMessage("/tuio/2Dblb");
            oscMessage.Append("set");
            oscMessage.Append(id); // s
            oscMessage.Append(x); // x
            oscMessage.Append(y); // y
            oscMessage.Append(angle); // a
            oscMessage.Append(width); // w
            oscMessage.Append(height); // h
            oscMessage.Append(area); // f
            oscMessage.Append(velocityX); // X
            oscMessage.Append(velocityY); // Y
            oscMessage.Append(rotationVelocity); // A
            oscMessage.Append(acceleration); // m
            oscMessage.Append(rotationAcceleration); // r

            isSendRequired = true;
        }

        new public void Update(float x, float y, float angle, float width, float height, float area, float velocityX, float velocityY, float rotationVelocity, float acceleration, float rotationAcceleration)
        {
            bool changed = !(Math.Abs(x - X) <= precision) ||
                !(Math.Abs(y - Y) <= precision) ||
                !(Math.Abs(angle - Angle) <= precision) ||
                !(Math.Abs(width - Width) <= precision) ||
                !(Math.Abs(height - Height) <= precision) ||
                !(Math.Abs(area - Area) <= precision) ||
                !(Math.Abs(velocityX - VelocityX) <= precision) ||
                !(Math.Abs(velocityY - VelocityY) <= precision) ||
                !(Math.Abs(rotationVelocity - RotationVelocity) <= precision) ||
                !(Math.Abs(acceleration - Acceleration) <= precision) ||
                !(Math.Abs(rotationAcceleration - RotationAcceleration) <= precision);

            if (changed)
            {
                base.Update(x, y, angle, width, height, area, velocityX, velocityY, rotationVelocity, acceleration, rotationAcceleration);

                UpdateOSCMessage();
                isSendRequired = true;

                if (onUpdated != null) onUpdated(this);
            }
        }

        protected void UpdateOSCMessage()
        {
            oscMessage.UpdateDataAt(2, X);
            oscMessage.UpdateDataAt(3, Y);
            oscMessage.UpdateDataAt(4, Angle);
            oscMessage.UpdateDataAt(5, Width);
            oscMessage.UpdateDataAt(6, Height);
            oscMessage.UpdateDataAt(7, Area);
            oscMessage.UpdateDataAt(8, VelocityX);
            oscMessage.UpdateDataAt(9, VelocityY);
            oscMessage.UpdateDataAt(10, RotationVelocity);
            oscMessage.UpdateDataAt(11, Acceleration);
            oscMessage.UpdateDataAt(12, RotationAcceleration);
        }
    }
}
