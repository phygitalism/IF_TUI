using System;
using OSCsharp.Data;
using TUIOsharp.Entities;

namespace RecognitionService.Tuio.Entities
{
    public class TUIOCursor : TuioCursor, ITUIOEntity
    {
        private const double precision = 1e-5;
        public OscMessage oscMessage { get; private set; }
        public bool isSendRequired { get; set; }

        public event EntityUpdatedHandler onUpdated;


        public TUIOCursor(int id) : this(id, 0, 0, 0, 0, 0) { }

        public TUIOCursor(int id, float x, float y, float velocityX, float velocityY, float acceleration)
            : base(id, x, y, velocityX, velocityY, acceleration)
        {

            // http://www.tuio.org/?specification - Profiles
            // /tuio/2Dcur set s x y X Y m
            oscMessage = new OscMessage("/tuio/2Dcur");
            oscMessage.Append("set");
            oscMessage.Append(id); // s
            oscMessage.Append(x); // x
            oscMessage.Append(y); // y
            oscMessage.Append(velocityX); // X
            oscMessage.Append(velocityY); // Y
            oscMessage.Append(acceleration); // m

            isSendRequired = true;
        }

        new public void Update(float x, float y, float velocityX, float velocityY, float acceleration)
        {
            bool changed = !(Math.Abs(x - X) <= precision) ||
                !(Math.Abs(y - Y) <= precision) ||
                !(Math.Abs(velocityX - VelocityX) <= precision) ||
                !(Math.Abs(velocityY - VelocityY) <= precision) ||
                !(Math.Abs(acceleration - Acceleration) <= precision);

            if (changed)
            {
                base.Update(x, y, velocityX, velocityY, acceleration);

                UpdateOSCMessage();
                isSendRequired = true;

                if (onUpdated != null) onUpdated(this);
            }
        }

        protected void UpdateOSCMessage()
        {
            oscMessage.UpdateDataAt(2, X);
            oscMessage.UpdateDataAt(3, Y);
            oscMessage.UpdateDataAt(4, VelocityX);
            oscMessage.UpdateDataAt(5, VelocityY);
            oscMessage.UpdateDataAt(6, Acceleration);
        }
    }
}
