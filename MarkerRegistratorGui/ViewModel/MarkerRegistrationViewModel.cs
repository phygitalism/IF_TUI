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
		private readonly IMarkerRegistrationField _registrationField;
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

			_registrationField = registrationField;

			FieldPosition = Observable.Return(_registrationField.FieldPosition)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			FieldSize = Observable.Return(_registrationField.FiledSize)
				.ScaleToScreen(scaleAdapter)
				.ToReactiveProperty();

			IsCandidatePlaced = Observable.Merge(
				Observable.FromEvent(
					h => _registrationField.OnMarkerCandidatePlaced += h,
					h => _registrationField.OnMarkerCandidatePlaced -= h
				)
				.Select(_ => true),
				Observable.FromEvent(
					h => _registrationField.OnMarkerCandidateRemoved += h,
					h => _registrationField.OnMarkerCandidateRemoved -= h
				)
				.Select(_ => false)
			)
			.ToReactiveProperty();

			_disposable = new CompositeDisposable()
			{
				IdSelectionPanel.SelectedId
					.Subscribe(registrationService.RegisterId)
			};

			IsSelectingId = IsCandidatePlaced;
		}

		public void Dispose() => _disposable.Dispose();
	}
}
