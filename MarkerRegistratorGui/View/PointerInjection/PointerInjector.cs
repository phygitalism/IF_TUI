using System;

namespace MarkerRegistratorGui.View.PointerInjection
{
	public static class PointerInjector
	{
		static PointerInjector()
		{
			if (!TouchInjector.InitializeTouchInjection(255, TouchFeedback.INDIRECT))
				throw new Exception("Couldn't initialize touch injection");
		}

		public static void InjectPointerDown(int id, int x, int y)
			=> InjectPointer(id, x, y, PointerFlags.INRANGE | PointerFlags.DOWN);

		public static void InjectPointerUpdate(int id, int x, int y)
			=> InjectPointer(id, x, y, PointerFlags.INRANGE | PointerFlags.UPDATE);

		public static void InjectPointerUp(int id, int x, int y)
			=> InjectPointer(id, x, y, PointerFlags.INRANGE | PointerFlags.UP);

		private static void InjectPointer(int id, int x, int y, PointerFlags flags)
		{
			var pointerInfo = new PointerTouchInfo()
			{
				PointerInfo =
				{
					PointerId = (uint)id,
					PointerType = PointerInputType.TOUCH,
					PointerFlags = flags,
					PtPixelLocation =
					{
						X = x,
						Y = y
					}
				}
			};

			if (!TouchInjector.InjectTouchInput(1, new[] { pointerInfo }))
				throw new Exception("Couldn't inject");
		}
	}
}
