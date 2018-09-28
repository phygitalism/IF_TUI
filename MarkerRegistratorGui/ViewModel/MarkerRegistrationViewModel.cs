using System.Diagnostics;
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

		public ReactiveProperty<IdSelectionViewModel> IdSelection { get; }
			= new ReactiveProperty<IdSelectionViewModel>();

		public MarkerRegistrationViewModel(IMarkerRegistrationService registrationService, ScaleAdapter scaleAdapter)
		{
			_registrationService = registrationService;

			FieldPosition = Observable.Return(_registrationService.RegistrationField.position)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			FieldSize = Observable.Return(_registrationService.RegistrationField.size)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			SelectIdAsync();
		}

		private async void SelectIdAsync()
		{
			IdSelection.Value = new IdSelectionViewModel(_registrationService.IdsCount);

			var selectedId = await IdSelection.Value.SelectedId.FirstAsync();
			Debug.Print($"Selected id {selectedId}");

			IdSelection.Value = null;
		}
	}
}
