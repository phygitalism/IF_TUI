using System;
using System.Collections.Generic;

using RecognitionService.Models;

namespace RecognitionService.Recognition
{
    public interface ITangibleMarkerRecognizer
    {
        List<RecognizedTangibleMarker> RecognizeTangibleMarkers(
            List<TouchPoint> validTouches,
            List<RegistredTangibleMarker> knownMarkers,
            ref Dictionary<int, int> markersTouches
        );
    }
}
