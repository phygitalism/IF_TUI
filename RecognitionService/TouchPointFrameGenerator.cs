using System;
using System.IO;
using System.Numerics;
using System.Collections.Generic;
using System.Windows.Threading;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

using RecognitionService.Models;

namespace RecognitionService
{
    public class TouchPointFrameGenerator : IInputProvider, IDisposable
    {
        private const int ScreenWidth = 3840;
        private const int ScreenHeight = 2160;

        private DispatcherTimer _statusTimer;

        private StreamReader file;
        private int counter = 0;

        public event Action<TouchPointFrame> OnTouchesRecieved;

        public TouchPointFrameGenerator()
        {
            var fileName = "test.txt";
            var basePath = Environment.CurrentDirectory;
            var filePath = Path.Combine(basePath, fileName);

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
                TimeSpan.FromSeconds(1),
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

            var token = JToken.Parse(json); // Removes \" symbols
            var touchesArray = JArray.FromObject(token);

            var touchPoints = new List<TouchPoint>();
            foreach (var touchData in touchesArray)
            {
                var touchJson = JToken.Parse((string)touchData)[0];
                var tp = JObject.Parse((string)touchJson).ToObject<PQMultiTouch.PQMTClientImport.TouchPoint>();
                
                var relativePosition = new Vector2(tp.x / ScreenWidth, tp.y / ScreenHeight);
                var relativeAcceleration = new Vector2(tp.dx / ScreenWidth, tp.dy / ScreenHeight);
                var touchPoint = new TouchPoint(tp.id, relativePosition, relativeAcceleration, (TouchPoint.ActionType)tp.point_event);
                touchPoints.Add(touchPoint);
            }

            return new TouchPointFrame(frameId, timestamp, touchPoints);
        }

        public void Dispose()
        {
            file.Close();
            Console.WriteLine("There were {0} lines.", counter);
        }
    }
}
