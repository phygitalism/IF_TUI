using System;
using System.IO;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Numerics;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PQ = PQMultiTouch.PQMTClientImport;

using EPQT_Error = PQMultiTouch.PQMTClientImport.EnumPQErrorType;
using EPQT_TPoint = PQMultiTouch.PQMTClientImport.EnumPQTouchPointType;
using EPQT_TRequest = PQMultiTouch.PQMTClientImport.EnumTouchClientRequestType;

using RecognitionService.Models;

namespace RecognitionService.Input.Touch
{
    class СonnectionServerException : Exception
    {
        public СonnectionServerException(string message) : base(message) { }
    }

    class TouchOverlay : IDeviceController, IInputProvider, IDisposable
    {
        public string DeviceName { get; private set; } = "PQ LABS Touch Overlay";
        public DeviceState State { get; private set; } = DeviceState.Uninitialized;
        public event Action<DeviceState> OnStateChanged;

        public event Action<TouchPointFrame> OnTouchesRecieved;

        public float ScreenWidth { get; private set; }
        public float ScreenHeight { get; private set; }

        // use static delegate to prevent the GC collect the CallBack functors;
        private static PQ.PFuncOnReceivePointFrame OnReceivePointFrameFunc;
        private static PQ.PFuncOnServerBreak OnServerBreakFunc;
        private static PQ.PFuncOnReceiveError OnReceiveErrorFunc;
        private static PQ.PFuncOnGetDeviceInfo OnGetDeviceInfoFunc;

        public TouchOverlay() { }

        public void Init()
        {
            if (State == DeviceState.Uninitialized)
            {
                BindToTouchOverlayEvents();

                try
                {
                    ConnectToServer();
                    State = DeviceState.Initialized;
                    OnStateChanged?.Invoke(State);
                }
                catch (СonnectionServerException ex)
                {
                    Console.WriteLine("Unable connect to device.");
                    Console.WriteLine(ex);
                    OnStateChanged?.Invoke(DeviceState.Uninitialized);
                }
            }
        }

        public void Start()
        {
            if (State == DeviceState.Initialized)
            {
                State = DeviceState.Starting;
                State = DeviceState.Running;
                OnStateChanged?.Invoke(State);
            }
        }

        public void Stop()
        {
            if (State == DeviceState.Running)
            {
                State = DeviceState.Initialized;
                OnStateChanged?.Invoke(State);
            }
        }

        public void Terminate()
        {
            Stop();
            Console.WriteLine("disconnect server...");
            PQ.DisconnectServer();
            State = DeviceState.Uninitialized;
        }

        public void Dispose()
        {
            Terminate();
        }

        private void BindToTouchOverlayEvents()
        {
            OnReceivePointFrameFunc = new PQ.PFuncOnReceivePointFrame(OnReceivePointFrame);
            OnServerBreakFunc = new PQ.PFuncOnServerBreak(OnServerBreak);
            OnReceiveErrorFunc = new PQ.PFuncOnReceiveError(OnReceiveError);
            OnGetDeviceInfoFunc = new PQ.PFuncOnGetDeviceInfo(OnGetDeviceInfo);

            PQ.SetOnReceivePointFrame(OnReceivePointFrameFunc, IntPtr.Zero);
            PQ.SetOnServerBreak(OnServerBreakFunc, IntPtr.Zero);
            PQ.SetOnReceiveError(OnReceiveErrorFunc, IntPtr.Zero);
            PQ.SetOnGetDeviceInfo(OnGetDeviceInfoFunc, IntPtr.Zero);
        }

        private void ConnectToServer()
        {
            int err_code = (int)EPQT_Error.PQMTE_SUCCESS;
            string local_ip = "127.0.0.1";

            Console.WriteLine("connect to server...");
            if ((err_code = PQ.ConnectServer(local_ip, PQ.PQMT_DEFAULT_CLIENT_PORT)) != (int)EPQT_Error.PQMTE_SUCCESS)
            {
                throw new СonnectionServerException($"connect server fail, socket errror code:{err_code}");
            }

            Console.WriteLine("connect success, send request.");
            PQ.TouchClientRequest tcq = new PQ.TouchClientRequest();
            tcq.type = (int)EPQT_TRequest.RQST_RAWDATA_ALL | (int)EPQT_TRequest.RQST_GESTURE_ALL;

            if ((err_code = PQ.SendRequest(ref tcq)) != (int)EPQT_Error.PQMTE_SUCCESS)
            {
                throw new СonnectionServerException($"send request fail, errror code:{err_code}");
            }

            if ((err_code = PQ.GetServerResolution(OnGetServerResolution, IntPtr.Zero)) != (int)EPQT_Error.PQMTE_SUCCESS)
            {
                throw new СonnectionServerException($"get server resolution fail, errror code:{err_code}");
            }

            Console.WriteLine("send request success, start recv.");
        }

        private void OnReceivePointFrame(int frameId, int timestamp, int movingPointCount, IntPtr movingPointArray, IntPtr callbackObject)
        {
            //Console.WriteLine($"frame_id:{frameId},time_stamp:{timestamp} ms,moving point count:{movingPointCount}");

            var touchPoints = new List<TouchPoint>();
            for (int i = 0; i < movingPointCount; ++i)
            {
                IntPtr p_tp = (IntPtr)(movingPointArray.ToInt64() + i * Marshal.SizeOf(typeof(PQ.TouchPoint)));
                PQ.TouchPoint tp = (PQ.TouchPoint)Marshal.PtrToStructure(p_tp, typeof(PQ.TouchPoint));

                var relativePosition = new Vector2(tp.x, tp.y);
                var relativeAcceleration = new Vector2(tp.dx, tp.dy);
                var touchPoint = new TouchPoint(tp.id, relativePosition, relativeAcceleration, (TouchPoint.ActionType)tp.point_event);
                touchPoints.Add(touchPoint);
            }

            OnTouchesRecieved?.Invoke(new TouchPointFrame(frameId, timestamp, touchPoints));
        }

        private void OnServerBreak(IntPtr param, IntPtr callbackObject)
        {
            Console.WriteLine("server break, disconnect here");
            PQ.DisconnectServer();
        }

        private void OnGetServerResolution(int width, int height, IntPtr callbackObject)
        {
            ScreenWidth = width;
            ScreenHeight = height;
            Console.WriteLine($"server resolution:{width},{height}");
        }

        private void OnReceiveError(int errorCode, IntPtr callbackObject)
        {
            switch (errorCode)
            {
                case (int)EPQT_Error.PQMTE_RCV_INVALIDATE_DATA:
                    Console.WriteLine(" error: receive invalidate data.");
                    break;
                case (int)EPQT_Error.PQMTE_SERVER_VERSION_OLD:
                    Console.WriteLine(" error: the multi-touch server is old for this client, please update the multi-touch server.");
                    break;
                case (int)EPQT_Error.PQMTE_EXCEPTION_FROM_CALLBACKFUNCTION:
                    Console.WriteLine(" **** some exceptions thrown from the call back functions.");
                    break;
                default:
                    Console.WriteLine($" socket error, socket error code:{errorCode}");
                    break;
            }
        }

        private void OnGetDeviceInfo(ref PQ.TouchDeviceInfo deviceInfo, IntPtr callbackObject)
        {
            Console.WriteLine($" touch screen, SerialNumber:{deviceInfo.serial_number},({deviceInfo.screen_width},{deviceInfo.screen_height}).");
        }
    }
}
