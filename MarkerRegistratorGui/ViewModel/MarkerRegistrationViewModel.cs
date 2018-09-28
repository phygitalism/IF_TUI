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
			= new ReactiveProperty<Vector2>();
		public ReactiveProperty<Vector2> FieldSize { get; }
			= new ReactiveProperty<Vector2>();

		public ReactiveProperty<IdSelectionViewModel> IdSelection { get; }
			= new ReactiveProperty<IdSelectionViewModel>();

		public MarkerRegistrationViewModel(IMarkerRegistrationService registrationService)
		{
			_registrationService = registrationService;

			FieldPosition.Value = _registrationService.RegistrationField.position;
			FieldSize.Value = _registrationService.RegistrationField.size;

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
