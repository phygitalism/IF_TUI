using Reactive.Bindings;

namespace MarkerRegistratorGui.ViewModel
{
	public class SelectableIdViewModel
	{
		public int Id { get; }
		public ReactiveCommand SelectedCommand { get; }

		public SelectableIdViewModel(int id)
		{
			Id = id;
			SelectedCommand = new ReactiveCommand();
		}
	}
}
