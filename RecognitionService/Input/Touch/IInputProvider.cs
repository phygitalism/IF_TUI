using System;
using RecognitionService.Models;

namespace RecognitionService.Input.Touch
{
    public interface IInputProvider
    {
        event Action<TouchPointFrame> OnTouchesRecieved;
    }
}
