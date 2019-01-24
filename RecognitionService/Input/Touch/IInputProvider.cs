using System;
using RecognitionService.Models;

namespace RecognitionService.Input.Touch
{
    public interface IInputProvider
    {
        float ScreenWidth { get; }
        float ScreenHeight { get; }

        event Action<TouchPointFrame> OnTouchesRecieved;
    }
}
