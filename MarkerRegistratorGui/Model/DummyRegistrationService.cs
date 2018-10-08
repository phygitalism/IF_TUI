using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace MarkerRegistratorGui.Model
{
	public class DummyRegistrationService : IMarkerRegistrationService
	{
		private readonly HashSet<int> _availableIds = new HashSet<int>(Enumerable.Range(0, 10));
		private readonly HashSet<int> _registeredIds = new HashSet<int>();

		public IEnumerable<int> AvailableIds => _availableIds.Where(id => !_registeredIds.Contains(id));
		public IEnumerable<int> RegisteredIds => _registeredIds;

		public void RegisterId(int id)
		{
			if (!AvailableIds.Contains(id))
				throw new InvalidOperationException();

			_registeredIds.Add(id);
		}

		public void UnregisterId(int id)
		{
			if (!RegisteredIds.Contains(id))
				throw new InvalidOperationException();

			_registeredIds.Remove(id);
		}
	}
}
