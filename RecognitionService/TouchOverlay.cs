using System;
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
    class TouchOverlay : IDeviceController
    {
        public string DeviceName { get; private set; }
        public DeviceState State { get; private set; }
        public delegate void StateChangedEvent();
        public event StateChangedEvent OnStateChanged;

        private static PQ.PFuncOnReceivePointFrame cur_rf_func = new PQ.PFuncOnReceivePointFrame(OnReceivePointFrame);
        private static PQ.PFuncOnServerBreak cur_svr_break = new PQ.PFuncOnServerBreak(OnServerBreak);
        private static PQ.PFuncOnReceiveError cur_rcv_err_func = new PQ.PFuncOnReceiveError(OnReceiveError);
        private static PQ.PFuncOnGetServerResolution cur_get_resolution = new PQ.PFuncOnGetServerResolution(OnGetServerResolution);
        private static PQ.PFuncOnGetDeviceInfo cur_fn_get_dev_info = new PQ.PFuncOnGetDeviceInfo(OnGetDeviceInfo);

        public TouchOverlay()
        {
            DeviceName = "PQ LABS Touch Overlay";
            State = DeviceState.Uninitialized;
        }

        public void Init()
        {
            if (State == DeviceState.Uninitialized)
            {
                BindToTouchOverlayEvents();
                int err_code = TouchOverlay.ConnectToServer();
                if (err_code != (int)PQ.EnumPQErrorType.PQMTE_SUCCESS)
                {
                    Console.WriteLine("Unable connect to device.");
                    return;
                }
                State = DeviceState.Initialized;
                OnStateChanged?.Invoke();
            }
        }

        public void Start()
        {
            if (State == DeviceState.Initialized)
            {
                State = DeviceState.Starting;
                OnStateChanged?.Invoke();
            }
        }

        public void Stop()
        {
            if (State == DeviceState.Running)
            {
                State = DeviceState.Initialized;
                OnStateChanged?.Invoke();
            }
        }

        public void Terminate()
        {
            Stop();
            Console.WriteLine("disconnect server...");
            PQ.DisconnectServer();
            State = DeviceState.Uninitialized;
        }


        static private int ConnectToServer()
        {
            int err_code = (int)EPQT_Error.PQMTE_SUCCESS;
            try
            {
                string local_ip = "127.0.0.1";

                Console.WriteLine(" connect to server...");
                if ((err_code = PQ.ConnectServer(local_ip, PQ.PQMT_DEFAULT_CLIENT_PORT)) != (int)EPQT_Error.PQMTE_SUCCESS)
                {
                    Console.WriteLine(" connect server fail, socket errror code:{0}", err_code);
                    return err_code;
                }

                Console.WriteLine(" connect success, send request.");
                PQ.TouchClientRequest tcq = new PQ.TouchClientRequest();
                tcq.type = (int)EPQT_TRequest.RQST_RAWDATA_ALL |
                    (int)EPQT_TRequest.RQST_GESTURE_ALL;

                if ((err_code = PQ.SendRequest(ref tcq)) != (int)EPQT_Error.PQMTE_SUCCESS)
                {
                    Console.WriteLine(" send request fail, error code:{0}", err_code);
                    return err_code;
                }

                if ((err_code = PQ.GetServerResolution(OnGetServerResolution, IntPtr.Zero)) != (int)EPQT_Error.PQMTE_SUCCESS)
                {
                    Console.WriteLine(" get server resolution fail,error code:{0}", err_code);
                    return err_code;
                }
                // start receiving
                Console.WriteLine(" send request success, start recv.");

            }
            catch (Exception ex)
            {
                Console.WriteLine(" exception: {0}", ex.Message);
            }
            return err_code;
        }

        private void BindToTouchOverlayEvents()
        {
            PQ.SetOnReceivePointFrame(cur_rf_func, IntPtr.Zero);
            // PQ.SetOnReceiveGesture(cur_rg_func, IntPtr.Zero);
            PQ.SetOnServerBreak(cur_svr_break, IntPtr.Zero);
            PQ.SetOnReceiveError(cur_rcv_err_func, IntPtr.Zero);
            PQ.SetOnGetDeviceInfo(cur_fn_get_dev_info, IntPtr.Zero);
        }

        static void OnReceivePointFrame(int frame_id, int time_stamp, int moving_point_count, IntPtr moving_point_array, IntPtr call_back_object)
        {
            string[] tp_event = new string[3]
            {
                "down",
                "move",
                "up"
            };
            Console.WriteLine("frame_id:{0},time_stamp:{1} ms,moving point count:{2}", frame_id, time_stamp, moving_point_count);
            for (int i = 0; i < moving_point_count; ++i)
            {
                IntPtr p_tp = (IntPtr)(moving_point_array.ToInt64() + i * Marshal.SizeOf(typeof(PQ.TouchPoint)));
                PQ.TouchPoint tp = (PQ.TouchPoint)Marshal.PtrToStructure(p_tp, typeof(PQ.TouchPoint));

                OnTouchPoint(ref tp);
            }
        }

        static void OnTouchPoint(ref PQ.TouchPoint tp)
        {
            switch ((EPQT_TPoint)tp.point_event)
            {
                case EPQT_TPoint.TP_DOWN:
                    Console.WriteLine("  point {0} come at ({1},{2}) width:{3} height:{4}", tp.id, tp.x, tp.y, tp.dx, tp.dy);
                    break;
                case EPQT_TPoint.TP_MOVE:
                    Console.WriteLine("  point {0} move at ({1},{2}) width:{3} height:{4}", tp.id, tp.x, tp.y, tp.dx, tp.dy);
                    break;
                case EPQT_TPoint.TP_UP:
                    Console.WriteLine("  point {0} leave at ({1},{2}) width:{3} height:{4}", tp.id, tp.x, tp.y, tp.dx, tp.dy);
                    break;
            }
        }

        static void OnServerBreak(IntPtr param, IntPtr call_back_object)
        {
            Console.WriteLine("server break, disconnect here");
            PQ.DisconnectServer();
        }
        static void OnGetServerResolution(int x, int y, IntPtr call_back_object)
        {
            Console.WriteLine("server resolution:{0},{1}", x, y);
        }

        static void OnReceiveError(int err_code, IntPtr call_back_object)
        {
            switch (err_code)
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
                    Console.WriteLine(" socket error, socket error code:{0}", err_code);
                    break;
            }
        }
        static void OnGetDeviceInfo(ref PQ.TouchDeviceInfo deviceInfo, IntPtr call_back_object)
        {
            Console.WriteLine(" touch screen, SerialNumber:{0},({1},{2}). ", deviceInfo.serial_number, deviceInfo.screen_width, deviceInfo.screen_height);
        }
    }
}
