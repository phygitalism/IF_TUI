using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class IdSelectionViewModel
	{
		public ObservableCollection<SelectableIdViewModel> SelectableIds { get; }

		public ReactiveProperty<int> SelectedId { get; }

		public IdSelectionViewModel(int idsCount)
		{
			var selectableIds = Enumerable.Range(0, idsCount)
				.Select(id => new SelectableIdViewModel(id));

			SelectableIds = new ObservableCollection<SelectableIdViewModel>(selectableIds);

			SelectedId = SelectableIds.Select(
				selectable => selectable.SelectedCommand.Select(_ => selectable.Id)
			)
			.Merge()
			.ToReactiveProperty();
		}
	}
}
