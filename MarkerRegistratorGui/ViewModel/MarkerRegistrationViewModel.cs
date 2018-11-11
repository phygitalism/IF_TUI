using System;
using System.Diagnostics;
using System.Numerics;
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
		public ReactiveProperty<bool> IsCandidatePlaced { get; }
		public ReactiveProperty<bool> IsSelectingId { get; }

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
				.Do(value => Debug.WriteLine($"-> Candidate update = {value}"))
				.Select(state => state == MarkerCandidateState.Detected)
				.ToReactiveProperty();

			IsSelectingId = IsCandidatePlaced
				.SelectMany(async isSelecting =>
				{
					if (isSelecting)
					{
						await IdSelectionPanel.UpdateRegisteredIdsAsync();
						return IsCandidatePlaced.Value;
					}
					return false;
				})
				.ToReactiveProperty();

			_disposable = IdSelectionPanel.SelectedId
				.Subscribe(id =>
				{
					registrationService.RegisterCandidate(id);
					IsSelectingId.Value = false;
					IsCandidatePlaced.ForceNotify();
				});
		}

		public void Dispose() => _disposable.Dispose();
	}
}
