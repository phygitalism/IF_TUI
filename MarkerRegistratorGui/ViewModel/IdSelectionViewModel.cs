using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using MarkerRegistratorGui.Model;

namespace MarkerRegistratorGui.ViewModel
{
	public class IdSelectionViewModel
	{
		private readonly IMarkerRegistrationService _registrationService;

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
		}
	}
}
