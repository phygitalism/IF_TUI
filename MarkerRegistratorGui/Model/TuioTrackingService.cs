using System;
using System.Diagnostics;
using System.Numerics;
using TUIO;

namespace MarkerRegistratorGui.Model
{
	public class TuioTrackingService : IMarkerTrackingService
	{
		private const float _markerRadius = 0.1f;

		private readonly TuioClient _tuioClient = new TuioClient();
		private readonly TuioListener _tuioListener = new TuioListener();

		public event Action<int> OnMarkerDown;
		public event Action<int> OnMarkerUp;
		public event Action<MarkerState> OnMarkerStateUpdate;

		public TuioTrackingService()
		{
			_tuioListener.OnObjectAdd += HandleObjectAdd;
			_tuioListener.OnObjectUpdate += HandleObjectUpdate;
			_tuioListener.OnObjectRemove += HandleObjectRemove;

			_tuioClient.addTuioListener(_tuioListener);
		}

		private void HandleObjectAdd(TuioObject obj)
		{
			Debug.WriteLine($"Object add {obj.SymbolID}", typeof(TuioTrackingService));

			OnMarkerDown?.Invoke(obj.SymbolID);
			HandleObjectUpdate(obj);
		}

		private void HandleObjectUpdate(TuioObject obj)
		{
			Trace.WriteLine($"Object update {obj.SymbolID} pos: {new Vector2(obj.X, obj.Y)}");

			OnMarkerStateUpdate?.Invoke(new MarkerState(
				obj.SymbolID,
				new Vector2(obj.X, obj.Y),
				obj.Angle,
				_markerRadius
			));
		}

		private void HandleObjectRemove(TuioObject obj)
		{
			Debug.WriteLine($"Object remove {obj.SymbolID}", typeof(TuioTrackingService));

			OnMarkerDown?.Invoke(obj.SymbolID);
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
