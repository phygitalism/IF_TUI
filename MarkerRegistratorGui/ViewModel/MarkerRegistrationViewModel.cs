using System.Numerics;
using System.Reactive.Linq;
using MarkerRegistratorGui.Model;
using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class MarkerRegistrationViewModel
	{
		private readonly IMarkerRegistrationService _registrationService;

		public ReactiveProperty<Vector2> FieldPosition { get; }
		public ReactiveProperty<Vector2> FieldSize { get; }

		public IdSelectionViewModel IdSelectionPanel { get; }
		public ReactiveProperty<bool> IsSelectingId { get; }

		public MarkerRegistrationViewModel(IMarkerRegistrationService registrationService, ScaleAdapter scaleAdapter)
		{
			_registrationService = registrationService;

			IdSelectionPanel = new IdSelectionViewModel(registrationService.IdsCount);

			FieldPosition = Observable.Return(_registrationService.RegistrationField.position)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			FieldSize = Observable.Return(_registrationService.RegistrationField.size)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			IsSelectingId = Observable.Return(true)
				.Merge(IdSelectionPanel.SelectedId.Select(_ => false))
				.ToReactiveProperty();
		}
	}
}
