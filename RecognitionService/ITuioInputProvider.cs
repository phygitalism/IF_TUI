using System;
using System.Collections.Generic;

using RecognitionService.Models;

namespace RecognitionService
{
    public interface ITuioInputProvider
    {
        event Action<List<TouchPoint>, List<RecognizedTangibleMarker>> OnTuioInput;
    }
}
