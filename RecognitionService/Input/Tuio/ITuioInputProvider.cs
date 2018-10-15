using System;
using System.Collections.Generic;

using RecognitionService.Models;

namespace RecognitionService.Input.Tuio
{
    public interface ITuioInputProvider
    {
        event Action<List<TouchPoint>, List<RecognizedTangibleMarker>> OnTuioInput;
    }
}
