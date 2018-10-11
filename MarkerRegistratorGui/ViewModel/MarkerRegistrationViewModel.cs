using System;
using System.Numerics;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using MarkerRegistratorGui.Model;
using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class MarkerRegistrationViewModel : IDisposable
	{
		private readonly IDisposable _disposable;

		public ReactiveProperty<Vector2> FieldPosition { get; }
		public ReactiveProperty<Vector2> FieldSize { get; }

		public IdSelectionViewModel IdSelectionPanel { get; }
		public ReactiveProperty<bool> IsSelectingId { get; }
		public ReactiveProperty<bool> IsCandidatePlaced { get; }


		public MarkerRegistrationViewModel(
			IMarkerRegistrationService registrationService,
			IMarkerRegistrationField registrationField,
			ScaleAdapter scaleAdapter
		)
		{
			IdSelectionPanel = new IdSelectionViewModel(registrationService);

			FieldPosition = Observable.Return(registrationField.FieldPosition)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			FieldSize = Observable.Return(registrationField.FieldSize)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			IsCandidatePlaced = Observable.FromEvent<MarkerCandidateState>(
				h => registrationService.OnMarkerCandidateUpdated += h,
				h => registrationService.OnMarkerCandidateUpdated -= h
			)
			.Select(state => state == MarkerCandidateState.Detected)
			.ToReactiveProperty();

			_disposable = new CompositeDisposable()
			{
				IdSelectionPanel,
				IdSelectionPanel.SelectedId
					.Subscribe(registrationService.RegisterCandidate)
			};

			IsSelectingId = IsCandidatePlaced;
		}

		public void Dispose() => _disposable.Dispose();
	}
}
