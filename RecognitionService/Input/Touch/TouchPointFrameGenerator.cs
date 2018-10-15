using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RecognitionService.Models;

namespace RecognitionService.Input.Touch
{
    public class TouchPointFrameGenerator : IInputProvider, IDisposable
    {
        private const float ScreenWidth = 3840;
        private const float ScreenHeight = 2160;

        private DispatcherTimer _statusTimer;

        private StreamReader file;
        private int counter = 0;

        public event Action<TouchPointFrame> OnTouchesRecieved;

        public TouchPointFrameGenerator()
        {
            var fileName = "TestInput2.txt";
            var basePath = Environment.CurrentDirectory;
            var filePath = Path.Combine(basePath, "Resources", fileName);

            Console.WriteLine($"PATH {filePath}");

            try
            {
                this.file = new StreamReader(filePath);
                StartTimer();
            }
            catch (FileNotFoundException ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void StartTimer()
        {
            _statusTimer = new DispatcherTimer(
                TimeSpan.FromMilliseconds(100),
                DispatcherPriority.Normal,
                (sender, eventArgs) =>
                {
                    var line = file.ReadLine();
                    if (line != null)
                    {
                        Console.WriteLine(line);
                        counter++;

                        var touchPointFrame = Parse(line);
                        OnTouchesRecieved?.Invoke(touchPointFrame);
                    }
                    else
                    {
                        KillTimer();
                        file.Close();
                        Console.WriteLine("There were {0} lines.", counter);
                    }
                },
                Dispatcher.CurrentDispatcher
            );
        }

        private TouchPointFrame Parse(string line)
        {
            var splittedLine = line.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            var frameId = int.Parse(splittedLine[0]);
            var timestamp = int.Parse(splittedLine[1]);

            var json = splittedLine[3];
            for (var i = 4; i < splittedLine.Length; i++)
            {
                json += "," + splittedLine[i];
            }

            var touchesArray = JToken.Parse(json).Value<JArray>(); // Removes \" symbols

            var touchPoints = new List<TouchPoint>();
            foreach (var touchData in touchesArray)
            {
                var touchObject = JToken.Parse((string)touchData).Value<JObject>();
                var tp = touchObject.ToObject<PQMultiTouch.PQMTClientImport.TouchPoint>();
                
                var relativePosition = new Vector2(tp.x / ScreenWidth, tp.y / ScreenHeight);
                var relativeAcceleration = new Vector2(tp.dx / ScreenWidth, tp.dy / ScreenHeight);
                var touchPoint = new TouchPoint(tp.id, relativePosition, relativeAcceleration, (TouchPoint.ActionType)tp.point_event);
                touchPoints.Add(touchPoint);
            }

            return new TouchPointFrame(frameId, timestamp, touchPoints);
        }

        private void KillTimer()
        {
            if (_statusTimer != null)
            {
                _statusTimer.Stop();
                _statusTimer = null;
            }
        }

        public void Dispose()
        {
            KillTimer();
            file.Close();
            Console.WriteLine("There were {0} lines.", counter);
        }
    }
}
