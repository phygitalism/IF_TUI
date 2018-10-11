using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RecognitionService.Api;

namespace MarkerRegistratorGui.Model
{
	public class RegistrationService : IMarkerRegistrationService
	{
		private readonly List<int> _registeredIds = new List<int>();

		private readonly RecognitionServiceClient _serviceClient
			= new RecognitionServiceClient("ws://localhost:8080");

		private readonly IMarkerRegistrationField _registrationField;

		public IEnumerable<int> AvailableIds { get; } = Enumerable.Range(0, 20);
		public IEnumerable<int> RegisteredIds => _registeredIds;

		public event Action OnRegisteredIdsChanged;

		public RegistrationService(IMarkerRegistrationField registrationField)
		{
			_registrationField = registrationField;
		}

		public async Task UpdateRegisteredAsync()
		{
			var registered = await _serviceClient.GetMarkerListAsync();
		}

		public void RegisterId(int id)
		{
			var pointers = _registrationField.PointersInside.ToArray();

			if (pointers.Length != 3)
				throw new InvalidOperationException("Not 3 pointers");

			var triangle = new Triangle(pointers[0], pointers[1], pointers[2]);

			_serviceClient.RegisterMarker(id, triangle);
		}

		public void UnregisterId(int id)
		{
			if (!RegisteredIds.Contains(id))
				throw new InvalidOperationException($"Id {id} is not registered");

			_serviceClient.UnregisterMarker(id);
		}
	}
}
