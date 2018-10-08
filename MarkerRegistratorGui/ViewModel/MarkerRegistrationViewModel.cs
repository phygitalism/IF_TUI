using System.Numerics;
using System.Reactive.Linq;
using MarkerRegistratorGui.Model;
using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class MarkerRegistrationViewModel
	{
		private readonly IMarkerRegistrationField _registrationField;

		public ReactiveProperty<Vector2> FieldPosition { get; }
		public ReactiveProperty<Vector2> FieldSize { get; }

		public IdSelectionViewModel IdSelectionPanel { get; }
		public ReactiveProperty<bool> IsSelectingId { get; }

		public MarkerRegistrationViewModel(
			IMarkerRegistrationService registrationService,
			IMarkerRegistrationField registrationField,
			ScaleAdapter scaleAdapter
		)
		{
			IdSelectionPanel = new IdSelectionViewModel(registrationService);

			_registrationField = registrationField;

			FieldPosition = Observable.Return(_registrationField.FieldPosition)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			FieldSize = Observable.Return(_registrationField.FiledSize)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			IsSelectingId = Observable.Return(true)
				.Merge(IdSelectionPanel.SelectedId.Select(_ => false))
				.ToReactiveProperty();
		}
	}
}
