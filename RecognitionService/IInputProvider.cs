using System;
using RecognitionService.Models;

namespace RecognitionService
{
    public interface IInputProvider
    {
        event Action<TouchPointFrame> OnTouchesRecieved;
    }
}
