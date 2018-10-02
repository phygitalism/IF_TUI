using System;
using System.Diagnostics;
using System.Numerics;
using TUIO;

namespace MarkerRegistratorGui.Model
{
	public class TuioTrackingService : ITrackingService
	{
		private const float _markerRadius = 0.1f;

		private readonly TuioClient _tuioClient = new TuioClient();
		private readonly TuioListener _tuioListener = new TuioListener();

		public event Action<TrackerEvent<PointerState>> OnPointerEvent;
		public event Action<TrackerEvent<MarkerState>> OnMarkerEvent;

		public TuioTrackingService()
		{
			_tuioListener.OnCursorAdd += obj => InvokePointerEvent(TrackerEventType.Down, obj);
			_tuioListener.OnCursorUpdate += obj => InvokePointerEvent(TrackerEventType.Update, obj);
			_tuioListener.OnCursorRemove += obj => InvokePointerEvent(TrackerEventType.Up, obj);

			_tuioListener.OnObjectAdd += obj => InvokeMarkerEvent(TrackerEventType.Down, obj);
			_tuioListener.OnObjectUpdate += obj => InvokeMarkerEvent(TrackerEventType.Update, obj);
			_tuioListener.OnObjectRemove += obj => InvokeMarkerEvent(TrackerEventType.Up, obj);

			_tuioClient.addTuioListener(_tuioListener);
		}

		private void InvokePointerEvent(TrackerEventType type, TuioCursor obj)
		{
			Debug.WriteLine($"Pointer {type} id:{obj.CursorID} {new Vector2(obj.X, obj.Y)}");

			OnPointerEvent?.Invoke(new TrackerEvent<PointerState>(
				obj.CursorID,
				type,
				new PointerState(new Vector2(obj.X, obj.Y))
			));
		}

		private void InvokeMarkerEvent(TrackerEventType type, TuioObject obj)
		{
			Debug.WriteLine($"Marker {type} id:{obj.SymbolID} {new Vector2(obj.X, obj.Y)}");

			OnMarkerEvent?.Invoke(new TrackerEvent<MarkerState>(
				obj.SymbolID,
				type,
				new MarkerState(new Vector2(obj.X, obj.Y), obj.Angle, _markerRadius)
			));
		}

		public void Start()
		{
			Debug.WriteLine("Connecting", typeof(TuioTrackingService));
			_tuioClient.connect();
		}

		public void Stop()
		{
			Debug.WriteLine("Disconnecting", typeof(TuioTrackingService));
			_tuioClient.disconnect();
		}

		private class TuioListener : TUIO.TuioListener
		{
			public event Action<TuioCursor> OnCursorAdd;
			public event Action<TuioCursor> OnCursorUpdate;
			public event Action<TuioCursor> OnCursorRemove;

			public event Action<TuioObject> OnObjectAdd;
			public event Action<TuioObject> OnObjectUpdate;
			public event Action<TuioObject> OnObjectRemove;

			public void addTuioBlob(TuioBlob tblb) { }
			public void updateTuioBlob(TuioBlob tblb) { }
			public void removeTuioBlob(TuioBlob tblb) { }

			public void addTuioCursor(TuioCursor tcur) => OnCursorAdd?.Invoke(tcur);
			public void updateTuioCursor(TuioCursor tcur) => OnCursorUpdate?.Invoke(tcur);
			public void removeTuioCursor(TuioCursor tcur) => OnCursorRemove?.Invoke(tcur);

			public void addTuioObject(TuioObject tobj) => OnObjectAdd?.Invoke(tobj);
			public void updateTuioObject(TuioObject tobj) => OnObjectUpdate?.Invoke(tobj);
			public void removeTuioObject(TuioObject tobj) => OnObjectRemove?.Invoke(tobj);

			public void refresh(TuioTime ftime) { }
		}
	}
}
