using System.Windows.Media;
using System.Reactive.Linq;
using System.Numerics;
using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class TrackedMarkerViewModel
	{
		private static readonly Color[] _colors = new[]
		{
			Colors.Red,
			Colors.Blue,
			Colors.Green,
			Colors.Brown,
			Colors.Orange,
			Colors.Magenta,
			Colors.Yellow
		};

		public int Index { get; }
		public Color Color { get; }

		public ReactiveProperty<Vector2> Position { get; }
		public ReactiveProperty<float> Rotation { get; }
		public ReactiveProperty<float> Radius { get; }

		public ReactiveProperty<Vector2> ScaledPosition { get; }
		public ReactiveProperty<float> ScaledRadius { get; }

		public TrackedMarkerViewModel(int index, MainViewModel mainViewModel)
		{
			Index = index;
			Color = _colors[index % _colors.Length];

			Position = new ReactiveProperty<Vector2>();
			Radius = new ReactiveProperty<float>();
			Rotation = new ReactiveProperty<float>();

			ScaledPosition = Observable.CombineLatest(
				Position,
				mainViewModel.WindowWidth,
				mainViewModel.WindowHeight,
				(position, width, height) => new Vector2(position.X * width, position.Y * height)
			)
			.ToReactiveProperty();

			ScaledRadius = Observable.CombineLatest(
				Radius,
				mainViewModel.WindowWidth,
				(radius, width) => radius * width
			)
			.ToReactiveProperty();
		}

		public void UpdateValues(Vector2 position, float rotation, float radius)
		{
			Position.Value = position;
			Rotation.Value = rotation;
			Radius.Value = radius;
		}
	}
}
