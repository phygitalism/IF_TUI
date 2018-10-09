using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class IdSelectionViewModel : IDisposable
	{
		private readonly IMarkerRegistrationService _registrationService;
		private readonly IDisposable _disposable;

		public ObservableCollection<SelectableIdViewModel> SelectableIds { get; }

		public IObservable<int> SelectedId { get; }

		public IdSelectionViewModel(IMarkerRegistrationService registrationService)
		{
			_registrationService = registrationService;

			var selectableIds = _registrationService.AvailableIds
				.Select(id => new SelectableIdViewModel(id));

			SelectableIds = new ObservableCollection<SelectableIdViewModel>(selectableIds);

			SelectedId = SelectableIds.Select(
				selectable => selectable.SelectedCommand.Select(_ => selectable.Id)
			)
			.Merge();

			_disposable = new CompositeDisposable()
			{
				Observable.FromEvent(
					h => _registrationService.OnRegisteredIdsChanged += h,
					h => _registrationService.OnRegisteredIdsChanged -= h
				)
				.Subscribe(_ => UpdateLockedIds())
			};

			UpdateLockedIds();
		}

		private void UpdateLockedIds()
		{
			foreach (var selectable in SelectableIds)
				selectable.Enabled.Value = !IsRegistered(selectable);
		}

		private bool IsRegistered(SelectableIdViewModel selectable)
			=> _registrationService.RegisteredIds.Contains(selectable.Id);

		public void Dispose() => _disposable.Dispose();
	}
}
