using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class SelectableIdViewModel
	{
		public int Id { get; }
		public ReactiveProperty<bool> IsSelected { get; }

		public ReactiveCommand SelectedCommand { get; }

		public SelectableIdViewModel(int id)
		{
			Id = id;
			IsSelected = new ReactiveProperty<bool>();

			SelectedCommand = new ReactiveCommand();
		}
	}
}
