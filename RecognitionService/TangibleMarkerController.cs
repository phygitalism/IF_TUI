using System;
using System.Numerics;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RecognitionService.Models;

namespace RecognitionService
{
	class TangibleMarkerController
	{

		public MarkerConfig Config { get; private set; }

		private Storage _storage = new Storage();

		public TangibleMarkerController()
		{
			Config = _storage.Load();
		}

		public void RegisterMarkerWithId(Triangle triangle, int id)
		{
			var tangible = new RegistredTangibleMarker(id, triangle, 0.0f, 0.0f);
			Config.Add(tangible);
			_storage.Save(Config);
		}

		public void UnregisterMarkerWithId(int id)
		{
			var tangible = Config.registredTangibles.Find(marker => marker.Id == id);
			Config.Remove(tangible);
			_storage.Save(Config);
		}

		public int[] GetAllRegistredIds()
		{
			Console.WriteLine("OnMarkerListRequested");
			return Config.registredTangibles.Select(marker => marker.Id).ToArray();
		}
	}
}
