using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class SelectableIdViewModel
	{
		public int Id { get; }
		public ReactiveProperty<bool> Enabled { get; }

		public ReactiveCommand SelectedCommand { get; }

		public SelectableIdViewModel(int id)
		{
			Id = id;
			Enabled = new ReactiveProperty<bool>();

			SelectedCommand = new ReactiveCommand();
		}
	}
}
