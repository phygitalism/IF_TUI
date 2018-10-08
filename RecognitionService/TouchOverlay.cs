﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Diagnostics;
using System.Runtime.InteropServices;
using PQ = PQMultiTouch.PQMTClientImport;

using EPQT_Error = PQMultiTouch.PQMTClientImport.EnumPQErrorType;
using EPQT_TGesture = PQMultiTouch.PQMTClientImport.EnumPQTouchGestureType;
using EPQT_TPoint = PQMultiTouch.PQMTClientImport.EnumPQTouchPointType;
using EPQT_TRequest = PQMultiTouch.PQMTClientImport.EnumTouchClientRequestType;

namespace RecognitionService
{
    class СonnectionServerFail : Exception
    {
        public СonnectionServerFail(string message) : base(message) { }
    }

    class TouchOverlay : IDeviceController
    {
        public string DeviceName { get; private set; } = "PQ LABS Touch Overlay";
        public DeviceState State { get; private set; } = DeviceState.Uninitialized;
        public event Action<DeviceState> OnStateChanged;

        public TouchOverlay() { }

        public void Init()
        {
            if (State == DeviceState.Uninitialized)
            {
                BindToTouchOverlayEvents();

                try
                {
                    TouchOverlay.ConnectToServer();
                    State = DeviceState.Initialized;
                    OnStateChanged?.Invoke(State);
                }
                catch (СonnectionServerFail ex)
                {
                    Console.WriteLine("Unable connect to device.");
                    Console.WriteLine(ex);
                }
            }
        }

        public void Start()
        {
            if (State == DeviceState.Initialized)
            {
                State = DeviceState.Starting;
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

        private void BindToTouchOverlayEvents()
        {
            PQ.SetOnReceivePointFrame(new PQ.PFuncOnReceivePointFrame(OnReceivePointFrame), IntPtr.Zero);
            PQ.SetOnServerBreak(new PQ.PFuncOnServerBreak(OnServerBreak), IntPtr.Zero);
            PQ.SetOnReceiveError(new PQ.PFuncOnReceiveError(OnReceiveError), IntPtr.Zero);
            PQ.SetOnGetDeviceInfo(new PQ.PFuncOnGetDeviceInfo(OnGetDeviceInfo), IntPtr.Zero);
        }

        private static void ConnectToServer()
        {
            int err_code = (int)EPQT_Error.PQMTE_SUCCESS;
            string local_ip = "127.0.0.1";

            Console.WriteLine("connect to server...");
            if ((err_code = PQ.ConnectServer(local_ip, PQ.PQMT_DEFAULT_CLIENT_PORT)) != (int)EPQT_Error.PQMTE_SUCCESS)
            {
                throw new СonnectionServerFail($"connect server fail, socket errror code:{err_code}");
            }

            Console.WriteLine("connect success, send request.");
            PQ.TouchClientRequest tcq = new PQ.TouchClientRequest();
            tcq.type = (int)EPQT_TRequest.RQST_RAWDATA_ALL | (int)EPQT_TRequest.RQST_GESTURE_ALL;

            if ((err_code = PQ.SendRequest(ref tcq)) != (int)EPQT_Error.PQMTE_SUCCESS)
            {
                throw new СonnectionServerFail($"send request fail, errror code:{err_code}");
            }

            if ((err_code = PQ.GetServerResolution(OnGetServerResolution, IntPtr.Zero)) != (int)EPQT_Error.PQMTE_SUCCESS)
            {
                throw new СonnectionServerFail($"get server resolution fail, errror code:{err_code}");
            }

            Console.WriteLine("send request success, start recv.");
        }

        private static void OnReceivePointFrame(int frameId, int timestamp, int movingPointCount, IntPtr movingPointArray, IntPtr callbackObject)
        {
            Console.WriteLine($"frame_id:{frameId},time_stamp:{timestamp} ms,moving point count:{movingPointCount}");
            for (int i = 0; i < movingPointCount; ++i)
            {
                IntPtr p_tp = (IntPtr)(movingPointArray.ToInt64() + i * Marshal.SizeOf(typeof(PQ.TouchPoint)));
                PQ.TouchPoint tp = (PQ.TouchPoint)Marshal.PtrToStructure(p_tp, typeof(PQ.TouchPoint));

                OnTouchPoint(tp);
            }
        }

        private static void OnTouchPoint(PQ.TouchPoint touchPoint)
        {
            switch ((EPQT_TPoint)touchPoint.point_event)
            {
                case EPQT_TPoint.TP_DOWN:
                    Console.WriteLine($"  point {touchPoint.id} come at ({touchPoint.x},{touchPoint.y}) width:{touchPoint.dx} height:{touchPoint.dy}");
                    break;
                case EPQT_TPoint.TP_MOVE:
                    Console.WriteLine($"  point {touchPoint.id} come at ({touchPoint.x},{touchPoint.y}) width:{touchPoint.dx} height:{touchPoint.dy}");
                    break;
                case EPQT_TPoint.TP_UP:
                    Console.WriteLine($"  point {touchPoint.id} come at ({touchPoint.x},{touchPoint.y}) width:{touchPoint.dx} height:{touchPoint.dy}");
                    break;
            }
        }

        private static void OnServerBreak(IntPtr param, IntPtr callbackObject)
        {
            Console.WriteLine("server break, disconnect here");
            PQ.DisconnectServer();
        }

        private static void OnGetServerResolution(int width, int height, IntPtr callbackObject)
        {
            Console.WriteLine($"server resolution:{width},{height}");
        }

        private static void OnReceiveError(int errorCode, IntPtr callbackObject)
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

        private static void OnGetDeviceInfo(ref PQ.TouchDeviceInfo deviceInfo, IntPtr call_back_object)
        {
            Console.WriteLine($" touch screen, SerialNumber:{deviceInfo.serial_number},({deviceInfo.screen_width},{deviceInfo.screen_height}). ");
        }
    }
}
