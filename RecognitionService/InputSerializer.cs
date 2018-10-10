using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RecognitionService.Models;

namespace RecognitionService
{
    public class InputSerializer : IDisposable
    {
        private StreamWriter _streamWriter = new StreamWriter("touchpoints.txt") { AutoFlush = true };
        private IInputProvider _inputProvider;

        public InputSerializer(IInputProvider inputProvider)
        {
            _streamWriter.WriteLine("INIT SESSION");
            inputProvider.OnTouchesRecieved += Serialize;
        }

        private void Serialize(TouchPointFrame frame)
        {
            var frameData = JsonConvert.SerializeObject(frame, Formatting.None);
            _streamWriter.WriteLine($"{frameData.ToString()}");
        }

        public void Dispose()
        {
            _inputProvider.OnTouchesRecieved -= Serialize;
            _streamWriter.WriteLine("END SESSION");
            _streamWriter.Close();
            _streamWriter.Dispose();
        }
    }
}
