using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RecognitionService.Models;

namespace RecognitionService
{
	public class TangibleMarkerController : IDisposable
	{
		public MarkerConfig Config { get; private set; }

		private Storage _storage = new Storage();

		public TangibleMarkerController()
		{
			Config = _storage.Load();
		}

		public void RegisterMarkerWithId(Triangle triangle, int id)
		{
			Console.WriteLine("RegisterMarkerWithId");
			var tangible = new RegistredTangibleMarker(id, triangle, 0.0f, 0.0f);
			if (!Config.IsRegistredWithId(tangible.Id))
			{
				Config.Add(tangible);
			}
			else
			{
				Config.Update(tangible);
			}
			_storage.Save(Config);
		}

		public void UnregisterMarkerWithId(int id)
		{
			Console.WriteLine("UnregisterMarkerWithId");
			if (!Config.IsRegistredWithId(id)) { return; }

			var tangible = Config.GetTangibleWithId(id);
			Config.Remove(tangible);
			_storage.Save(Config);
		}

		public int[] GetAllRegistredIds()
		{
			Console.WriteLine("OnMarkerListRequested");
			return Config.registredTangibles.Select(marker => marker.Id).ToArray();
		}

		public void Dispose()
		{
			_storage.Dispose();
		}
	}
}
