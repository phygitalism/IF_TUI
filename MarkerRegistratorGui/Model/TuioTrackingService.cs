using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using TUIO;

namespace MarkerRegistratorGui.Model
{
	using PointerEvent = TrackerEvent<PointerState>;
	using MarkerEvent = TrackerEvent<MarkerState>;

	public class TuioTrackingService : ITrackingService
	{
		private const float _markerRadius = 0.1f;

		private readonly TuioClient _tuioClient = new TuioClient();
		private readonly TuioListener _tuioListener = new TuioListener();

		private readonly List<PointerEvent> _pointerEvents = new List<PointerEvent>();
		private readonly List<MarkerEvent> _markerEvents = new List<MarkerEvent>();

		public event Action<TrackerEvents> OnEvents;

		public TuioTrackingService()
		{
			_tuioListener.OnCursorAdd += cursor => AddEvent(TrackerEventType.Down, cursor);
			_tuioListener.OnCursorUpdate += cursor => AddEvent(TrackerEventType.Update, cursor);
			_tuioListener.OnCursorRemove += cursor => AddEvent(TrackerEventType.Up, cursor);

			_tuioListener.OnObjectAdd += obj => AddEvent(TrackerEventType.Down, obj);
			_tuioListener.OnObjectUpdate += obj => AddEvent(TrackerEventType.Update, obj);
			_tuioListener.OnObjectRemove += obj => AddEvent(TrackerEventType.Up, obj);

			_tuioListener.OnRefresh += HandleMessageEnded;

			_tuioClient.addTuioListener(_tuioListener);
		}

		private void AddEvent(TrackerEventType eventType, TuioObject obj)
			=> _markerEvents.Add(CreateEvent(eventType, obj));

		private void AddEvent(TrackerEventType eventType, TuioCursor obj)
			=> _pointerEvents.Add(CreateEvent(eventType, obj));

		private void HandleMessageEnded(TuioTime time)
		{
			OnEvents?.Invoke(new TrackerEvents(
				_markerEvents.ToArray(),
				_pointerEvents.ToArray()
			));

			_pointerEvents.Clear();
			_markerEvents.Clear();
		}

		private TrackerEvent<MarkerState> CreateEvent(TrackerEventType type, TuioObject obj)
			=> new TrackerEvent<MarkerState>(
				obj.SymbolID,
				type,
				new MarkerState(
					new Vector2(obj.X, obj.Y),
					obj.Angle,
					_markerRadius
				)
			);

		private TrackerEvent<PointerState> CreateEvent(TrackerEventType type, TuioCursor obj)
			=> new TrackerEvent<PointerState>(
				obj.CursorID,
				type,
				new PointerState(new Vector2(obj.X, obj.Y))
			);

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

			public event Action<TuioTime> OnRefresh;

			public void addTuioBlob(TuioBlob tblb) { }
			public void updateTuioBlob(TuioBlob tblb) { }
			public void removeTuioBlob(TuioBlob tblb) { }

			public void addTuioCursor(TuioCursor tcur) => OnCursorAdd?.Invoke(tcur);
			public void updateTuioCursor(TuioCursor tcur) => OnCursorUpdate?.Invoke(tcur);
			public void removeTuioCursor(TuioCursor tcur) => OnCursorRemove?.Invoke(tcur);

			public void addTuioObject(TuioObject tobj) => OnObjectAdd?.Invoke(tobj);
			public void updateTuioObject(TuioObject tobj) => OnObjectUpdate?.Invoke(tobj);
			public void removeTuioObject(TuioObject tobj) => OnObjectRemove?.Invoke(tobj);

			public void refresh(TuioTime ftime) => OnRefresh?.Invoke(ftime);
		}
	}
}
