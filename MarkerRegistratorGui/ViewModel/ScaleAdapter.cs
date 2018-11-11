using System;
using System.Numerics;
using System.Reactive.Linq;
using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class ScaleAdapter
	{
		public ReactiveProperty<float> WindowWidth { get; }
			= new ReactiveProperty<float>();
		public ReactiveProperty<float> WindowHeight { get; }
			= new ReactiveProperty<float>();

		public ReactiveProperty<Vector2> WindowSize { get; }

		public ScaleAdapter()
		{
			WindowSize = Observable.CombineLatest(
				WindowWidth,
				WindowHeight,
				(width, height) => new Vector2(width, height)
			)
			.ToReactiveProperty();
		}
	}

	public static class ScaleAdapterExtensions
	{
		public static IObservable<Vector2> ScaleToScreen(
			this IObservable<Vector2> observable,
			ScaleAdapter scaleAdapter
		)
			=> observable
				.CombineLatest(
					scaleAdapter.WindowSize,
					(vector, windowSize) => new Vector2(
						vector.X * windowSize.X,
						vector.Y * windowSize.Y
					)
			);

		public static IObservable<float> ScaleToScreenWidth(
			this IObservable<float> observable,
			ScaleAdapter scaleAdapter
		)
			=> observable.CombineLatest(
				scaleAdapter.WindowSize,
				(value, windowSize) => value * windowSize.X
			);

		public static IObservable<float> ScaleToScreenHeight(
			this IObservable<float> observable,
			ScaleAdapter scaleAdapter
		)
			=> observable.CombineLatest(
				scaleAdapter.WindowSize,
				(value, windowSize) => value * windowSize.Y
			);
	}
}
