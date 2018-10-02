using System.Windows.Media;
using System.Reactive.Linq;
using System.Numerics;
using Reactive.Bindings;
using MarkerRegistratorGui.Model;

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

		public TrackedMarkerViewModel(int index, ScaleAdapter scaleAdapter)
		{
			Index = index;
			Color = _colors[index % _colors.Length];

			Position = new ReactiveProperty<Vector2>();
			Radius = new ReactiveProperty<float>();
			Rotation = new ReactiveProperty<float>();

			ScaledPosition = Observable.CombineLatest(
				Position,
				scaleAdapter.WindowWidth,
				scaleAdapter.WindowHeight,
				(position, width, height) => new Vector2(position.X * width, position.Y * height)
			)
			.ToReactiveProperty();

			ScaledRadius = Radius.ScaleToScreenHeight(scaleAdapter)
				.ToReactiveProperty();
		}

		public void UpdateValues(MarkerState state)
		{
			Position.Value = state.position;
			Rotation.Value = state.rotation;
			Radius.Value = state.radius;
		}
	}
}
