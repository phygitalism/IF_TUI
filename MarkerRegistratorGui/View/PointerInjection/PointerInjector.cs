using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.View.PointerInjection
{
	public static class PointerInjector
	{
		static PointerInjector()
		{
			if (!TouchInjector.InitializeTouchInjection(255, TouchFeedback.INDIRECT))
				throw new Exception("Couldn't initialize touch injection");
		}

		public static void InjectPointers(IEnumerable<PointerUpdate> updates)
		{
			var touchInfo = updates
				.Select(CreatePointerTouchInfo)
				.ToArray();

			if (touchInfo.Length != 0 && !TouchInjector.InjectTouchInput(touchInfo.Length, touchInfo))
				throw new Win32Exception();
		}

		private static PointerTouchInfo CreatePointerTouchInfo(PointerUpdate update)
			=> new PointerTouchInfo()
			{
				PointerInfo =
					{
						PointerId = (uint)update.id,
						PointerType = PointerInputType.TOUCH,
						PointerFlags = GetFlags(update.e),
						PtPixelLocation =
						{
							X = update.point.X,
							Y = update.point.Y
						}
					}
			};

		private static PointerFlags GetFlags(TrackerEventType e)
		{
			switch (e)
			{
				case TrackerEventType.Down:
					return PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.DOWN;
				case TrackerEventType.Update:
					return PointerFlags.INRANGE | PointerFlags.INCONTACT | PointerFlags.UPDATE;
				case TrackerEventType.Up:
					return PointerFlags.UP;
				default:
					throw new NotImplementedException();
			}
		}

		public readonly struct PointerUpdate
		{
			public readonly int id;
			public readonly Point point;
			public readonly TrackerEventType e;

			public PointerUpdate(int id, Point point, TrackerEventType e)
			{
				this.id = id;
				this.point = point;
				this.e = e;
			}
		}
	}
}
